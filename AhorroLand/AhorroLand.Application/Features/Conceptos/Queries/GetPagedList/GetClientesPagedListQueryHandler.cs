using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Conceptos.Queries;

/// <summary>
/// Manejador concreto para la consulta de lista paginada de Conceptos.
/// </summary>
public sealed class GetConceptosPagedListQueryHandler
    : GetPagedListQueryHandler<Concepto, ConceptoDto, GetConceptosPagedListQuery>
{
    public GetConceptosPagedListQueryHandler(
        IReadRepositoryWithDto<Concepto, ConceptoDto> repository,
        ICacheService cacheService)
    : base(repository, cacheService)
    {
    }
}