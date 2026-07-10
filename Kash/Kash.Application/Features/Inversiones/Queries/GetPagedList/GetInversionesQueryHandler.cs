using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
