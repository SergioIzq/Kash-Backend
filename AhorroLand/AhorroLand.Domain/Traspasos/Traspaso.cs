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


    public CuentaId CuentaOrigenId { get; }
    public CuentaId CuentaDestinoId { get; }

    public Cantidad Importe { get; }
    public FechaRegistro Fecha { get; }
    public UsuarioId UsuarioId { get; }

    public Descripcion? Descripcion { get; }
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

        traspaso.AddDomainEvent(new TraspasoRegistradoDomainEvent(traspaso.Id.Value, cuentaOrigen.Value, cuentaDestino.Value, importe));

        return traspaso;
    }
}