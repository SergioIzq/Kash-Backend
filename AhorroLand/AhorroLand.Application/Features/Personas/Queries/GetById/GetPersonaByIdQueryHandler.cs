using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Personas.Queries;

public sealed class GetPersonaByIdQueryHandler
    : GetByIdQueryHandler<Persona, PersonaDto, GetPersonaByIdQuery>
{
    public GetPersonaByIdQueryHandler(
        IReadRepositoryWithDto<Persona, PersonaDto> repository,
  ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}