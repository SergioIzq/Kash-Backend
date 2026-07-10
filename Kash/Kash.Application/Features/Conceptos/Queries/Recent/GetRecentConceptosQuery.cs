using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Queries.Recent;

public sealed record GetRecentConceptosQuery : GetRecentQuery<Concepto, ConceptoDto, ConceptoId>
{
    public GetRecentConceptosQuery(int limit = 5, string? categoriaId = null) : base(limit)
    {
        CategoriaId = categoriaId;
    }

    public string? CategoriaId { get; }
}
