using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Filtros opcionales y combinables para la exportación de Gastos. Un filtro con lista vacía
/// o nula no restringe el resultado por ese campo.
/// </summary>
public sealed record GastoExportFiltro(
    DateTime? FechaInicio,
    DateTime? FechaFin,
    string? SearchTerm,
    Guid[]? ConceptoIds,
    Guid[]? CategoriaIds,
    Guid[]? ProveedorIds,
    Guid[]? PersonaIds);

/// <summary>
/// Listado completo (sin paginar) de Gastos de un usuario que cumplen un conjunto de filtros
/// combinables. Vive en Application (no en Domain, que no referencia Kash.Shared.Application
/// donde vive <see cref="GastoDto"/>), mismo patrón que <see cref="IConceptoPaginadoRepository"/>.
/// </summary>
public interface IGastoExportRepository
{
    Task<IReadOnlyList<GastoDto>> GetForExportAsync(Guid usuarioId, GastoExportFiltro filtro, CancellationToken cancellationToken = default);
}
