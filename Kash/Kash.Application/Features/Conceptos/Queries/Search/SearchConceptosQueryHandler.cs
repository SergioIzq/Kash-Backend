using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Queries.Search;

public sealed class SearchConceptosQueryHandler
    : SearchForAutocompleteQueryHandler<Concepto, ConceptoDto, SearchConceptosQuery, ConceptoId>
{
    public SearchConceptosQueryHandler(
        IReadRepository<Concepto, ConceptoDto, ConceptoId> repository,
        ICacheService cacheService)
    : base(repository, cacheService)
    {
    }

    // Sobrescribimos el Hook para inyectar el filtro de categoría
    protected override Dictionary<string, object>? GetCustomFilters(SearchConceptosQuery query)
    {
        if (string.IsNullOrEmpty(query.CategoriaId))
        {
            return null;
        }

        // Usamos el alias 'c' porque tu ConceptoReadRepository define GetTableAlias() => "c"
        return new Dictionary<string, object>
        {
            { "c.id_categoria", query.CategoriaId }
        };
    }
}
