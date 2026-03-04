using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.FormasPago.Queries;

public sealed class GetFormasPagoPagedListQueryHandler
    : GetPagedListQueryHandler<FormaPago, FormaPagoId, FormaPagoDto, GetFormasPagoPagedListQuery>
{
    public GetFormasPagoPagedListQueryHandler(
      IReadRepository<FormaPago, FormaPagoDto, FormaPagoId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}