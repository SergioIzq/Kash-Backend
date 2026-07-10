using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Queries;

/// <summary>
/// Manejador concreto para la consulta de lista paginada de Reglas de Categorización.
/// </summary>
public sealed class GetReglasCategorizacionPagedListQueryHandler
    : GetPagedListQueryHandler<ReglaCategorizacion, ReglaCategorizacionId, ReglaCategorizacionDto, GetReglasCategorizacionPagedListQuery>
{
    public GetReglasCategorizacionPagedListQueryHandler(
        IReadRepository<ReglaCategorizacion, ReglaCategorizacionDto, ReglaCategorizacionId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }

    protected override async Task<PagedList<ReglaCategorizacionDto>> ApplyFiltersAsync(
        GetReglasCategorizacionPagedListQuery query,
        CancellationToken cancellationToken)
    {
        if (query.UsuarioId.HasValue)
        {
            return await _dtoRepository.GetPagedReadModelsByUserAsync(
                query.UsuarioId.Value,
                query.Page,
                query.PageSize,
                query.SearchTerm,
                query.SortColumn,
                query.SortOrder,
                cancellationToken);
        }

        return null!;
    }
}
