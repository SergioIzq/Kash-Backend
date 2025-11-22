using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Conceptos.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Concepto.
/// </summary>
public sealed class GetConceptoByIdQueryHandler
    : GetByIdQueryHandler<Concepto, ConceptoDto, GetConceptoByIdQuery>
{
    public GetConceptoByIdQueryHandler(
        ICacheService cacheService,
        IReadRepositoryWithDto<Concepto, ConceptoDto> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}