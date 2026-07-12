using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
