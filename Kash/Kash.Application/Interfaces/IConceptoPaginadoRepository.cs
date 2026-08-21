using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Interfaces;

/// <summary>
/// Listado paginado de Conceptos filtrado por Categoría. Vive en Application (no en
/// <c>Kash.Domain</c>, que no referencia <c>Kash.Shared.Application</c> donde vive
/// <see cref="ConceptoDto"/>) porque <see cref="IReadRepository{T,TDto,TId}.GetPagedReadModelsByUserAsync"/>
/// del kernel no acepta filtros extra, mismo patrón que <see cref="IGastoHabitualesRepository"/>.
/// </summary>
public interface IConceptoPaginadoRepository
{
    Task<PagedList<ConceptoDto>> GetPagedByCategoriaAsync(
        Guid usuarioId,
        Guid categoriaId,
        int page,
        int pageSize,
        string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        CancellationToken cancellationToken = default);
}
