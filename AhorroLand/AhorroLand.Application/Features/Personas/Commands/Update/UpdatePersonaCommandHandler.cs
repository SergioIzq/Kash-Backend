using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Personas.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Persona.
/// </summary>
public sealed class UpdatePersonaCommandHandler
    : AbsUpdateCommandHandler<Persona, PersonaDto, UpdatePersonaCommand>
{
    public UpdatePersonaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Persona> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<Persona, PersonaDto> readOnlyRepository
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override void ApplyChanges(Persona entity, UpdatePersonaCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = new Nombre(command.Nombre);

        entity.Update(
            nuevoNombreVO
        );
    }
}
