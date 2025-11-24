using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain;

public sealed class Ingreso : AbsEntity
{
    // Constructor privado sin parámetros para EF Core
    private Ingreso() : base(Guid.Empty)
    {
    }

    private Ingreso(
        Guid id,
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
        var Ingreso = new Ingreso(
            Guid.NewGuid(),
            importe,
            fecha,
            conceptoId,
            clienteId,
            personaId,
            cuentaId,
            formaPagoId,
            usuarioId,
            descripcion);

        return Ingreso;
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