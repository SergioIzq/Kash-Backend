using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Proveedores.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~70 líneas a ~30 líneas (60% menos código).
/// </summary>
public sealed class CreateProveedorCommandHandler
    : AbsCreateCommandHandler<Proveedor, ProveedorId, CreateProveedorCommand>
{
    private readonly IProveedorWriteRepository _proveedorWriteRepository;

    public CreateProveedorCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Proveedor, ProveedorId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        IProveedorWriteRepository proveedorWriteRepository)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _proveedorWriteRepository = proveedorWriteRepository;
    }

    /// <summary>
    /// HOOK: Crea la entidad de dominio.
    /// Solo necesita implementar la lógica de creación, el resto lo maneja la clase base.
    /// </summary>
    protected override Proveedor CreateEntity(CreateProveedorCommand command, Dictionary<string, object>? dependencies = null)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        return Proveedor.Create(nombreVO, usuarioId);
    }

    /// <summary>
    /// HOOK: Validación y adición al contexto.
    /// Usa CreateAsyncWithValidation que valida unicidad Y agrega la entidad.
    /// </summary>
    protected override async Task<(Result ValidationResult, bool EntityAdded)> ValidateAndAddToContextAsync(
        Proveedor entity,
        CreateProveedorCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _proveedorWriteRepository.CreateAsyncWithValidation(entity, cancellationToken);
        return (result, result.IsSuccess); // Si es exitoso, la entidad fue agregada
    }
}
