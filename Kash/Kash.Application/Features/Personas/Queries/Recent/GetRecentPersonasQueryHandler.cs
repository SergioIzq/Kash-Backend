using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Personas.Queries.Recent;

public sealed class GetRecentPersonasQueryHandler
    : GetRecentQueryHandler<Persona, PersonaDto, PersonaId, GetRecentPersonasQuery>
{
    public GetRecentPersonasQueryHandler(
 IReadRepository<Persona, PersonaDto, PersonaId> repository,
    ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
