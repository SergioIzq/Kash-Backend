using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.IngresosProgramados.Commands;

public sealed record CreateIngresoProgramadoCommand : AbsCreateCommand<IngresoProgramado, IngresoProgramadoId>
{
    public required decimal Importe { get; init; }
    public required string Frecuencia { get; init; }
    public required DateTime? FechaEjecucion { get; init; }
    public string? Descripcion { get; init; }
    public required Guid ConceptoId { get; init; }
    public required string ConceptoNombre { get; init; }
    public Guid ClienteId { get; init; }
    public Guid CategoriaId { get; init; }
    public Guid PersonaId { get; init; }
    public required Guid CuentaId { get; init; }
    public required Guid FormaPagoId { get; init; }
    public bool Activo { get; init; } = true;
}
