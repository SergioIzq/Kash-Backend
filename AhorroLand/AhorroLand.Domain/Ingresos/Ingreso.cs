using AhorroLand.Domain.Ingresos.Eventos;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("ingresos")]
public sealed class Ingreso : AbsEntity<IngresoId>
{
  // Constructor privado sin parámetros para EF Core
    private Ingreso() : base(IngresoId.Create(Guid.NewGuid()).Value)
    {
    }

    private Ingreso(
        IngresoId id,
     Cantidad importe,
     FechaRegistro fecha,
      ConceptoId conceptoId,
        ClienteId clienteId,
   PersonaId personaId,
  CuentaId cuentaId,
        FormaPagoId formaPagoId,
  UsuarioId usuarioId,
        Descripcion? descripcion) : base(id)
    {
     Importe = importe;
Fecha = fecha;
        ConceptoId = conceptoId;
     ClienteId = clienteId;
  PersonaId = personaId;
        CuentaId = cuentaId;
  FormaPagoId = formaPagoId;
        UsuarioId = usuarioId;
        Descripcion = descripcion;
    }

    public Cantidad Importe { get; private set; }
public FechaRegistro Fecha { get; private set; }
    public Descripcion? Descripcion { get; private set; }

    public ConceptoId ConceptoId { get; private set; }
    public ClienteId ClienteId { get; private set; }
    public PersonaId PersonaId { get; private set; }
    public CuentaId CuentaId { get; private set; }
    public FormaPagoId FormaPagoId { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public Concepto Concepto { get; private set; } = null!;
    public Cliente Cliente { get; private set; } = null!;
    public Persona Persona { get; private set; } = null!;
    public Cuenta Cuenta { get; private set; } = null!;
 public FormaPago FormaPago { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    // El método Create genera el ID y no recibe los "Nombre"
    public static Ingreso Create(
   Cantidad importe,
 FechaRegistro fecha,
        ConceptoId conceptoId,
      ClienteId clienteId,
     PersonaId personaId,
        CuentaId cuentaId,
        FormaPagoId formaPagoId,
        UsuarioId usuarioId,
        Descripcion? descripcion)
    {
        var ingreso = new Ingreso(
            IngresoId.Create(Guid.NewGuid()).Value,
    importe,
          fecha,
            conceptoId,
      clienteId,
            personaId,
            cuentaId,
            formaPagoId,
  usuarioId,
  descripcion);

        // 🔥 Lanzar evento de dominio cuando se crea un ingreso
        ingreso.AddDomainEvent(new IngresoCreadoEvent(ingreso.Id, cuentaId, importe));

        return ingreso;
    }

    public void Update(
        Cantidad importe,
     FechaRegistro fecha,
        ConceptoId conceptoId,
        ClienteId clienteId,
        PersonaId personaId,
        CuentaId cuentaId,
        FormaPagoId formaPagoId,
        UsuarioId usuarioId,
        Descripcion? descripcion)
    {
        // 🔥 Guardar valores anteriores para el evento
     var cuentaIdAnterior = CuentaId;
      var importeAnterior = Importe;

      Importe = importe;
  Fecha = fecha;
        ConceptoId = conceptoId;
        ClienteId = clienteId;
        PersonaId = personaId;
        CuentaId = cuentaId;
        FormaPagoId = formaPagoId;
        UsuarioId = usuarioId;
    Descripcion = descripcion;

        // 🔥 Lanzar evento solo si cambió la cuenta o el importe
        if (!cuentaIdAnterior.Equals(cuentaId) || !importeAnterior.Equals(importe))
        {
       AddDomainEvent(new IngresoActualizadoEvent(
 Id, 
       cuentaIdAnterior, 
        importeAnterior, 
        cuentaId, 
        importe));
        }
    }

    /// <summary>
    /// Marca el ingreso como eliminado y lanza el evento de dominio.
    /// </summary>
    public void MarkAsDeleted()
    {
     // 🔥 Lanzar evento de dominio cuando se elimina un ingreso
        AddDomainEvent(new IngresoEliminadoEvent(Id, CuentaId, Importe));
    }
}