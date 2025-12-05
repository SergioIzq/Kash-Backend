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
}