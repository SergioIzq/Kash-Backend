using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Gastos.Commands;

public sealed record CreateGastoCommand : AbsCreateCommand<Gasto, GastoId>
{
    public required decimal Importe { get; init; }
    public required DateTime Fecha { get; init; }
    public string? Descripcion { get; init; }

    public required Guid CategoriaId { get; init; }
    public required Guid ConceptoId { get; init; }
    public required Guid ProveedorId { get; init; }
    public required Guid PersonaId { get; init; }
    public required Guid CuentaId { get; init; }
    public required Guid FormaPagoId { get; init; }
    public required Guid UsuarioId { get; init; }

    // ?? NUEVO: Nombres opcionales para auto-creación
    /// <summary>
    /// Nombre del concepto. Si ConceptoId no existe y se proporciona este valor,
    /// se creará automáticamente el concepto.
    /// </summary>
    public string? ConceptoNombre { get; init; }

    /// <summary>
    /// Nombre del proveedor. Si Proveedor no existe y se proporciona este valor,
    /// se creará automáticamente el proveedor.
    /// </summary>
    public string? ProveedorNombre { get; init; }

    /// <summary>
    /// Nombre de la persona. Si PersonaId no existe y se proporciona este valor,
    /// se creará automáticamente la persona.
    /// </summary>
    public string? PersonaNombre { get; init; }
}
