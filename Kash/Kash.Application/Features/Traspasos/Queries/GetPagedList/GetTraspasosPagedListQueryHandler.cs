using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Traspasos.Queries;

public sealed class GetTraspasosPagedListQueryHandler
    : GetPagedListQueryHandler<Traspaso, TraspasoId, TraspasoDto, GetTraspasosPagedListQuery>
{
    public GetTraspasosPagedListQueryHandler(
    IReadRepository<Traspaso, TraspasoDto, TraspasoId> repository,
   ICacheService cacheService)
      : base(repository, cacheService)
    {
    }
}
