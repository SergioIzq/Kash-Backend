using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
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
