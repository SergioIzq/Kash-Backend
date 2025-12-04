using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Personas.Commands;

public sealed class CreatePersonaCommandHandler : AbsCreateCommandHandler<Persona, PersonaId, CreatePersonaCommand>
{
    public CreatePersonaCommandHandler(
    IUnitOfWork unitOfWork,
    IWriteRepository<Persona, PersonaId> writeRepository,
    ICacheService cacheService)
    : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override Persona CreateEntity(CreatePersonaCommand command)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        var newPersona = Persona.Create(Guid.NewGuid(), nombreVO, usuarioId);
        return newPersona;
    }
}

