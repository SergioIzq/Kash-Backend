using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Queries.Recent;

public sealed class GetRecentConceptosQueryHandler
    : GetRecentQueryHandler<Concepto, ConceptoDto, ConceptoId, GetRecentConceptosQuery>
{
    public GetRecentConceptosQueryHandler(
         IReadRepository<Concepto, ConceptoDto, ConceptoId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }

    protected override Dictionary<string, object>? GetCustomFilters(GetRecentConceptosQuery query)
    {
        if (string.IsNullOrEmpty(query.CategoriaId))
        {
            return null;
        }

        // Usamos "c.id_categoria" porque tu ConceptoReadRepository define el alias "c"
        // y el filtro se inyecta en el WHERE principal.
        return new Dictionary<string, object>
        {
            { "c.id_categoria", query.CategoriaId }
        };
    }

    // Sobrescribimos para que la caché sea única por categoría
    protected override string GetCacheKeySuffix(GetRecentConceptosQuery query)
    {
        return string.IsNullOrEmpty(query.CategoriaId)
            ? string.Empty
            : $":cat_{query.CategoriaId}";
    }
}
