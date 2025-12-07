using AhorroLand.Domain.Traspasos.Eventos;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("traspasos")]
public sealed class Traspaso : AbsEntity<TraspasoId>
{
    private Traspaso() : base(TraspasoId.Create(Guid.NewGuid()).Value)
    {

    }

    private Traspaso(
        TraspasoId id,
        CuentaId cuentaOrigen,
        CuentaId cuentaDestino,
        Cantidad importe,
        FechaRegistro fecha,
        UsuarioId usuarioId,
        Descripcion? descripcion) : base(id)
    {
        CuentaOrigenId = cuentaOrigen;
        CuentaDestinoId = cuentaDestino;
        Importe = importe;
        Fecha = fecha;
        UsuarioId = usuarioId;
        Descripcion = descripcion;
    }


    public CuentaId CuentaOrigenId { get; private set; }
    public CuentaId CuentaDestinoId { get; private set; }

    public Cantidad Importe { get; private set; }
    public FechaRegistro Fecha { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public Cuenta CuentaOrigen { get; private set; } = null!;
    public Cuenta CuentaDestino { get; private set; } = null!;
    public Descripcion? Descripcion { get; private set; }
    
    public static Traspaso Create(
    CuentaId cuentaOrigen,
        CuentaId cuentaDestino,
        Cantidad importe,
        FechaRegistro fecha,
    UsuarioId usuarioId,
    Descripcion? descripcion)
    {
        // ⭐ Única validación de dominio intrínseca del Traspaso:
        if (cuentaOrigen.Equals(cuentaDestino))
        {
          throw new InvalidOperationException("La cuenta de origen y destino deben ser diferentes.");
      }

    var traspaso = new Traspaso(
          TraspasoId.Create(Guid.NewGuid()).Value,
            cuentaOrigen,
            cuentaDestino,
        importe,
   fecha,
       usuarioId,
    descripcion);

     // 🔥 Lanzar evento de dominio cuando se crea un traspaso
        traspaso.AddDomainEvent(new TraspasoCreadoEvent(
      traspaso.Id,
            cuentaOrigen,
cuentaDestino,
  importe));

        return traspaso;
    }

    public void Update(
        CuentaId cuentaOrigen,
  CuentaId cuentaDestino,
        Cantidad importe,
        FechaRegistro fecha,
    Descripcion? descripcion)
    {
      // ⭐ Validación de dominio
        if (cuentaOrigen.Equals(cuentaDestino))
        {
            throw new InvalidOperationException("La cuenta de origen y destino deben ser diferentes.");
        }

        // 🔥 Guardar valores anteriores para el evento
     var cuentaOrigenAnterior = CuentaOrigenId;
        var cuentaDestinoAnterior = CuentaDestinoId;
        var importeAnterior = Importe;

      CuentaOrigenId = cuentaOrigen;
        CuentaDestinoId = cuentaDestino;
     Importe = importe;
        Fecha = fecha;
        Descripcion = descripcion;

   // 🔥 Lanzar evento solo si cambió alguna cuenta o el importe
  if (!cuentaOrigenAnterior.Equals(cuentaOrigen) ||
            !cuentaDestinoAnterior.Equals(cuentaDestino) ||
            !importeAnterior.Equals(importe))
        {
  AddDomainEvent(new TraspasoActualizadoEvent(
     Id,
  cuentaOrigenAnterior,
           cuentaDestinoAnterior,
  importeAnterior,
      cuentaOrigen,
    cuentaDestino,
          importe));
        }
    }

    /// <summary>
  /// Marca el traspaso como eliminado y lanza el evento de dominio.
    /// </summary>
    public void MarkAsDeleted()
    {
        // 🔥 Lanzar evento de dominio cuando se elimina un traspaso
      AddDomainEvent(new TraspasoEliminadoEvent(
        Id,
      CuentaOrigenId,
          CuentaDestinoId,
            Importe));
    }
}