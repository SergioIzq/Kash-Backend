using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.FormasPago.Queries.Search;

/// <summary>
/// Handler para búsqueda rápida de formas de pago (autocomplete).
/// </summary>
public sealed class SearchFormasPagoQueryHandler
    : SearchForAutocompleteQueryHandler<FormaPago, FormaPagoDto, SearchFormasPagoQuery, FormaPagoId>
{
    public SearchFormasPagoQueryHandler(
        IReadRepository<FormaPago, FormaPagoDto, FormaPagoId> repository,
   ICacheService cacheService)
: base(repository, cacheService)
    {
    }
}
