using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Personas.Commands;

/// <summary>
/// Manejador concreto para eliminar una Persona.
/// Hereda toda la lógica de la clase base genérica.
/// </summary>
public sealed class DeletePersonaCommandHandler
    : DeleteCommandHandler<Persona, PersonaId, DeletePersonaCommand>
{
    public DeletePersonaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Persona, PersonaId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}


