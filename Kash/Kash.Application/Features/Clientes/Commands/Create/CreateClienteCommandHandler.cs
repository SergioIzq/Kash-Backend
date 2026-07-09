using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Clientes.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~70 líneas a ~30 líneas (60% menos código).
/// </summary>
public sealed class CreateClienteCommandHandler
    : AbsCreateCommandHandler<Cliente, ClienteId, CreateClienteCommand>
{
    private readonly IClienteWriteRepository _ClienteWriteRepository;

    public CreateClienteCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Cliente, ClienteId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IClienteWriteRepository ClienteWriteRepository)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _ClienteWriteRepository = ClienteWriteRepository;
    }

    /// <summary>
    /// HOOK: Crea la entidad de dominio.
    /// Solo necesita implementar la lógica de creación, el resto lo maneja la clase base.
    /// </summary>
    protected override Cliente CreateEntity(CreateClienteCommand command, Dictionary<string, object>? dependencies = null)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        return Cliente.Create(nombreVO, usuarioId);
    }

    /// <summary>
    /// HOOK: Validación y adición al contexto.
    /// Usa CreateAsyncWithValidation que valida unicidad Y agrega la entidad.
    /// </summary>
    protected override async Task<(Result ValidationResult, bool EntityAdded)> ValidateAndAddToContextAsync(
        Cliente entity,
        CreateClienteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _ClienteWriteRepository.CreateAsyncWithValidation(entity, cancellationToken);
        return (result, result.IsSuccess); // Si es exitoso, la entidad fue agregada
    }
}
