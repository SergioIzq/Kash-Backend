using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.FormasPago.Queries.Recent;

public sealed record GetRecentFormasPagoQuery : GetRecentQuery<FormaPago, FormaPagoDto, FormaPagoId>
{
    public GetRecentFormasPagoQuery(int limit = 5) : base(limit) { }
}
