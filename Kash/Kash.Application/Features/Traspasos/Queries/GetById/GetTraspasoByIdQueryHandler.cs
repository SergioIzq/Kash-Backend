using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
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
