using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Gastos.Commands;

/// <summary>
/// Manejador concreto para eliminar un Gasto.
/// </summary>
public sealed class DeleteGastoCommandHandler
    : DeleteCommandHandler<Gasto, DeleteGastoCommand>
{
    public DeleteGastoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Gasto> writeRepository,
        ICacheService cacheService)
        : base(unitOfWork, writeRepository, cacheService)
    {
    }
}


