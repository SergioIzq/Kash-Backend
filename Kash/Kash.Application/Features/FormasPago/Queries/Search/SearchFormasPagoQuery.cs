using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.FormasPago.Queries.Search;

/// <summary>
/// Query para búsqueda rápida de formas de pago (autocomplete).
/// </summary>
public sealed record SearchFormasPagoQuery : SearchForAutocompleteQuery<FormaPago, FormaPagoDto, FormaPagoId>
{
    public SearchFormasPagoQuery(string searchTerm, int limit = 10)
        : base(searchTerm, limit)
    {
    }
}
