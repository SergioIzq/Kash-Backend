using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Traspasos.Queries;

public sealed class GetTraspasoByIdQueryHandler
    : GetByIdQueryHandler<Traspaso, TraspasoId, TraspasoDto, GetTraspasoByIdQuery>
{
    public GetTraspasoByIdQueryHandler(
     ICacheService cacheService,
        IReadRepository<Traspaso, TraspasoDto, TraspasoId> readOnlyRepository
      )
        : base(readOnlyRepository, cacheService)
    {
    }
}
