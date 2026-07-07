using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.Results;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.TraspasosProgramados.Queries;

public sealed class GetTraspasosProgramadosPagedListQueryHandler
    : GetPagedListQueryHandler<TraspasoProgramado, TraspasoProgramadoId, TraspasoProgramadoDto, GetTraspasosProgramadosPagedListQuery>
{
    public GetTraspasosProgramadosPagedListQueryHandler(
        IReadRepository<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId> repository,
        ICacheService cacheService)
    : base(repository, cacheService)
    {
    }

    protected override async Task<PagedList<TraspasoProgramadoDto>> ApplyFiltersAsync(
    GetTraspasosProgramadosPagedListQuery query,
    CancellationToken cancellationToken)
    {
        // Si tenemos UsuarioId, usar el método optimizado con filtro
        if (query.UsuarioId.HasValue)
        {
            return await _dtoRepository.GetPagedReadModelsByUserAsync(
         query.UsuarioId.Value,
                       query.Page,
              query.PageSize,
              null, // searchTerm
           null, // sortColumn
          null, // sortOrder
             cancellationToken);
        }

        // Sin UsuarioId, dejamos que el handler base maneje
        return null!;
    }
}
