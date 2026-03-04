using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
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
