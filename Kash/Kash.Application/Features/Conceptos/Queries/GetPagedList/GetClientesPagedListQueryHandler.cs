using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Queries;

/// <summary>
/// Manejador concreto para la consulta de lista paginada de Conceptos.
/// </summary>
public sealed class GetConceptosPagedListQueryHandler
    : GetPagedListQueryHandler<Concepto, ConceptoId, ConceptoDto, GetConceptosPagedListQuery>
{
    public GetConceptosPagedListQueryHandler(
        IReadRepository<Concepto, ConceptoDto, ConceptoId> repository,
        ICacheService cacheService)
    : base(repository, cacheService)
    {
    }
}