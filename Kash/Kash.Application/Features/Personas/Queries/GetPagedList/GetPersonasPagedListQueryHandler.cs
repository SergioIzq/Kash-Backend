using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Personas.Queries;

public sealed class GetPersonasPagedListQueryHandler
  : GetPagedListQueryHandler<Persona, PersonaId, PersonaDto, GetPersonasPagedListQuery>
{
    public GetPersonasPagedListQueryHandler(
        IReadRepository<Persona, PersonaDto, PersonaId> repository,
    ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
