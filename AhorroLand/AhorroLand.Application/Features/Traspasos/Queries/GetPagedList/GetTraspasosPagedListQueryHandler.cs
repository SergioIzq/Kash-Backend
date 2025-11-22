using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Traspasos.Queries;

public sealed class GetTraspasosPagedListQueryHandler
    : GetPagedListQueryHandler<Traspaso, TraspasoDto, GetTraspasosPagedListQuery>
{
    public GetTraspasosPagedListQueryHandler(
    IReadRepositoryWithDto<Traspaso, TraspasoDto> repository,
   ICacheService cacheService)
      : base(repository, cacheService)
    {
    }
}
