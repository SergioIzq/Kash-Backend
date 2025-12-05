using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("gastos")]
public sealed class Gasto : AbsEntity<GastoId>
{
    // Constructor privado sin parámetros para EF Core
    private Gasto() : base(GastoId.Create(Guid.NewGuid()).Value)
    {
    }

    private Gasto(
     GastoId id,
     Cantidad importe,
      FechaRegistro fecha,
        ConceptoId conceptoId,
     ProveedorId proveedorId,
    PersonaId personaId,
    CuentaId cuentaId,
        FormaPagoId formaPagoId,
        UsuarioId usuarioId,
  Descripcion? descripcion) : base(id)
    {
        Importe = importe;
        Fecha = fecha;
        ConceptoId = conceptoId;
        ProveedorId = proveedorId;
        PersonaId = personaId;
        CuentaId = cuentaId;
        FormaPagoId = formaPagoId;
        UsuarioId = usuarioId;
        Descripcion = descripcion;
    }

    // --- Propiedades Puras del Dominio ---
    public Cantidad Importe { get; private set; }
    public FechaRegistro Fecha { get; private set; }
    public Descripcion? Descripcion { get; private set; }

    // --- IDs (Referencias a otros Agregados) ---
    public ConceptoId ConceptoId { get; private set; }
    public ProveedorId ProveedorId { get; private set; }
    public PersonaId PersonaId { get; private set; }
    public CuentaId CuentaId { get; private set; }
    public FormaPagoId FormaPagoId { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    // --- Detalles de Infraestructura (Solo para Proyecciones/Queries) ---
    public Concepto Concepto { get; private set; } = null!;
    public Proveedor Proveedor { get; private set; } = null!;
    public Persona Persona { get; private set; } = null!;
    public Cuenta Cuenta { get; private set; } = null!;
    public FormaPago FormaPago { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    // El método Factory (Create) sigue siendo PURO.
    public static Gasto Create(
        Cantidad importe,
FechaRegistro fecha,
   ConceptoId conceptoId,
        ProveedorId proveedorId,
        PersonaId personaId,
  CuentaId cuentaId,
        FormaPagoId formaPagoId,
      UsuarioId usuarioId,
        Descripcion? descripcion)
    {
        var gasto = new Gasto(
            GastoId.Create(Guid.NewGuid()).Value,
            importe,
     fecha,
   conceptoId,
     proveedorId,
            personaId,
   cuentaId,
    formaPagoId,
   usuarioId,
            descripcion);

        return gasto;
    }

    public void Update(
    Cantidad importe,
        FechaRegistro fecha,
    ConceptoId conceptoId,
        ProveedorId proveedorId,
    PersonaId personaId,
        CuentaId cuentaId,
    FormaPagoId formaPagoId,
        UsuarioId usuarioId,
        Descripcion? descripcion)
    {
        Importe = importe;
        Fecha = fecha;
        ConceptoId = conceptoId;
        ProveedorId = proveedorId;
        PersonaId = personaId;
        CuentaId = cuentaId;
        FormaPagoId = formaPagoId;
        UsuarioId = usuarioId;
        Descripcion = descripcion;
    }
}