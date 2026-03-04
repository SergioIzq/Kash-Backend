using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Conceptos.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Concepto.
/// </summary>
public sealed class GetConceptoByIdQueryHandler
    : GetByIdQueryHandler<Concepto, ConceptoId, ConceptoDto, GetConceptoByIdQuery>
{
    public GetConceptoByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<Concepto, ConceptoDto, ConceptoId> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}