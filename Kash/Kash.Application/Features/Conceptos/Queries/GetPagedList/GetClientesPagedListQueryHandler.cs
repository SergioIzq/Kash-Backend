using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
