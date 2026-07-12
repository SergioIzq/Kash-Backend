using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.FormasPago.Queries.Recent;

public sealed class GetRecentFormasPagoQueryHandler
    : GetRecentQueryHandler<FormaPago, FormaPagoDto, FormaPagoId, GetRecentFormasPagoQuery>
{
    public GetRecentFormasPagoQueryHandler(
        IReadRepository<FormaPago, FormaPagoDto, FormaPagoId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
