using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
