using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Filtros opcionales y combinables para la exportación de Ingresos. Un filtro con lista vacía
/// o nula no restringe el resultado por ese campo.
/// </summary>
public sealed record IngresoExportFiltro(
    DateTime? FechaInicio,
    DateTime? FechaFin,
    string? SearchTerm,
    Guid[]? ConceptoIds,
    Guid[]? CategoriaIds,
    Guid[]? ClienteIds,
    Guid[]? PersonaIds);

/// <summary>
/// Listado completo (sin paginar) de Ingresos de un usuario que cumplen un conjunto de filtros
/// combinables. Mismo patrón que <see cref="IGastoExportRepository"/>.
/// </summary>
public interface IIngresoExportRepository
{
    Task<IReadOnlyList<IngresoDto>> GetForExportAsync(Guid usuarioId, IngresoExportFiltro filtro, CancellationToken cancellationToken = default);
}
