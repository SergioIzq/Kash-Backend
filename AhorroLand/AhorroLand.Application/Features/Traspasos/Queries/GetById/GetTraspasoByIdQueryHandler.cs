using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Traspasos.Queries;

public sealed class GetTraspasoByIdQueryHandler
    : GetByIdQueryHandler<Traspaso, TraspasoDto, GetTraspasoByIdQuery>
{
    public GetTraspasoByIdQueryHandler(
     ICacheService cacheService,
        IReadRepositoryWithDto<Traspaso, TraspasoDto> readOnlyRepository
      )
        : base(readOnlyRepository, cacheService)
    {
    }
}
