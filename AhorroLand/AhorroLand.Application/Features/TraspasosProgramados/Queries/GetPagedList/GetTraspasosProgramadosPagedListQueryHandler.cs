using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.TraspasosProgramados.Queries;

public sealed class GetTraspasosProgramadosPagedListQueryHandler
    : GetPagedListQueryHandler<TraspasoProgramado, TraspasoProgramadoDto, GetTraspasosProgramadosPagedListQuery>
{
    public GetTraspasosProgramadosPagedListQueryHandler(
   IReadRepositoryWithDto<TraspasoProgramado, TraspasoProgramadoDto> repository,
     ICacheService cacheService)
  : base(repository, cacheService)
    {
    }
}
