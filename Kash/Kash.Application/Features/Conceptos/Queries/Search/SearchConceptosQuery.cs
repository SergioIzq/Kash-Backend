using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Queries.Search;

// 1. Añadimos la propiedad CategoriaId aquí
public sealed record SearchConceptosQuery(string SearchTerm, int Limit = 10)
    : SearchForAutocompleteQuery<Concepto, ConceptoDto, ConceptoId>(SearchTerm, Limit)
{
    public string? CategoriaId { get; init; }
}
