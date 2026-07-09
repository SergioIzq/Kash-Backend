using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Commands;

/// <summary>
/// REFACTORIZADO: Handler simplificado usando hooks de la clase base.
/// Reducido de ~80 líneas a ~35 líneas (56% menos código).
/// </summary>
public sealed class UpdateCuentaCommandHandler
    : AbsUpdateCommandHandler<Cuenta, CuentaId, CuentaDto, UpdateCuentaCommand>
{
    private readonly ICuentaWriteRepository _cuentaWriteRepository;

    public UpdateCuentaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Cuenta, CuentaId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext,
        ICuentaWriteRepository cuentaWriteRepository)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _cuentaWriteRepository = cuentaWriteRepository;
    }

    /// <summary>
    /// HOOK: Aplica los cambios del comando a la entidad.
    /// </summary>
    protected override void ApplyChanges(Cuenta entity, UpdateCuentaCommand command, Dictionary<string, object>? dependencies = null)
    {
        var nuevoNombreVO = Nombre.Create(command.Nombre).Value;

        entity.Update(nuevoNombreVO);
    }

    /// <summary>
    /// HOOK: Validación y actualización con repositorio específico.
    /// Valida unicidad del nombre y marca la entidad como modificada.
    /// </summary>
    protected override async Task<(Result ValidationResult, bool EntityUpdated)> ValidateAndUpdateInContextAsync(
        Cuenta entity,
        UpdateCuentaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _cuentaWriteRepository.UpdateAsync(entity, cancellationToken);
        return (result, result.IsSuccess); // Si es exitoso, la entidad fue marcada como modificada
    }
}
