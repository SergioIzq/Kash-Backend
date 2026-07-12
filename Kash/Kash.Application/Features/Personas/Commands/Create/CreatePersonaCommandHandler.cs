using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Personas.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~70 líneas a ~30 líneas (60% menos código).
/// </summary>
public sealed class CreatePersonaCommandHandler
    : AbsCreateCommandHandler<Persona, PersonaId, CreatePersonaCommand>
{
    private readonly IPersonaWriteRepository _personaWriteRepository;

    public CreatePersonaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Persona, PersonaId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IPersonaWriteRepository personaWriteRepository)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _personaWriteRepository = personaWriteRepository;
    }

    /// <summary>
    /// HOOK: Crea la entidad de dominio.
    /// Solo necesita implementar la lógica de creación, el resto lo maneja la clase base.
    /// </summary>
    protected override Persona CreateEntity(CreatePersonaCommand command, Dictionary<string, object>? dependencies = null)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        return Persona.Create(nombreVO, usuarioId);
    }

    /// <summary>
    /// HOOK: Validación y adición al contexto.
    /// Usa CreateAsyncWithValidation que valida unicidad Y agrega la entidad.
    /// </summary>
    protected override async Task<(Result ValidationResult, bool EntityAdded)> ValidateAndAddToContextAsync(
        Persona entity,
        CreatePersonaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _personaWriteRepository.CreateAsyncWithValidation(entity, cancellationToken);
        return (result, result.IsSuccess); // Si es exitoso, la entidad fue agregada
    }
}
