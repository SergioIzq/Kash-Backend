using AhorroLand.Application.Features.Clientes.Queries;
using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.Results;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.TraspasosProgramados.Queries;

public sealed class GetTraspasosProgramadosPagedListQueryHandler
    : GetPagedListQueryHandler<TraspasoProgramado, TraspasoProgramadoId, TraspasoProgramadoDto, GetTraspasosProgramadosPagedListQuery>
{
    public GetTraspasosProgramadosPagedListQueryHandler(
        IReadRepositoryWithDto<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId> repository,
        ICacheService cacheService)
    : base(repository, cacheService)
    {
    }

    protected override async Task<PagedList<TraspasoProgramadoDto>> ApplyFiltersAsync(
    GetTraspasosProgramadosPagedListQuery query,
    CancellationToken cancellationToken)
    {
        // 🔥 Si tenemos UsuarioId, usar el método optimizado con filtro
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
