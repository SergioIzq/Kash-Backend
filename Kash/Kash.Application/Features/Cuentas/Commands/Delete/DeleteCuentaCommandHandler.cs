using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Commands;

/// <summary>
/// Manejador concreto para eliminar una Cuenta.
/// Hereda toda la lógica de la clase base genérica.
/// </summary>
public sealed class DeleteCuentaCommandHandler
    : DeleteCommandHandler<Cuenta, CuentaId, DeleteCuentaCommand>
{
    public DeleteCuentaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Cuenta, CuentaId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}


