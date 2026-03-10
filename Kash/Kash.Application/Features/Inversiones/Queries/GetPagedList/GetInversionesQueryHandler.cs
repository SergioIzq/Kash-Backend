using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Inversiones.Queries;

public sealed class GetInversionesQueryHandler
    : GetPagedListQueryHandler<Inversion, InversionId, InversionDto, GetInversionesQuery>
{
    public GetInversionesQueryHandler(
        IReadRepository<Inversion, InversionDto, InversionId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
