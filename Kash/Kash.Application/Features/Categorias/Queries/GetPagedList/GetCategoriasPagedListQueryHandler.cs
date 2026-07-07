using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.Results;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Queries;

/// <summary>
/// Manejador concreto para la consulta de lista paginada de Categorías.
/// Implementa la lógica específica de filtrado y ordenación.
/// </summary>
public sealed class GetCategoriasPagedListQueryHandler
    : GetPagedListQueryHandler<Categoria, CategoriaId, CategoriaDto, GetCategoriasPagedListQuery>
{
    public GetCategoriasPagedListQueryHandler(
        IReadRepository<Categoria, CategoriaDto, CategoriaId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }

    /// <summary>
    /// OPTIMIZADO: Usa método específico del repositorio que filtra por usuario.
    /// </summary>
    protected override async Task<PagedList<CategoriaDto>> ApplyFiltersAsync(
        GetCategoriasPagedListQuery query,
        CancellationToken cancellationToken)
    {
        // Si tenemos UsuarioId, usar el método optimizado con filtro
        if (query.UsuarioId.HasValue)
        {
            return await _dtoRepository.GetPagedReadModelsByUserAsync(
         query.UsuarioId.Value,
                       query.Page,
              query.PageSize,
              query.SearchTerm, // searchTerm
           query.SortColumn, // sortColumn
          query.SortOrder, // sortOrder
             cancellationToken);
        }

        // Sin UsuarioId, dejamos que el handler base maneje
        return null!;
    }
}
