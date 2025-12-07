using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Gastos.Commands;

/// <summary>
/// Manejador concreto para eliminar un Gasto.
/// 🔥 Sobrescribe LoadEntityForDeletionAsync para cargar la entidad y disparar el evento MarkAsDeleted.
/// </summary>
public sealed class DeleteGastoCommandHandler
    : DeleteCommandHandler<Gasto, GastoId, DeleteGastoCommand>
{
    public DeleteGastoCommandHandler(
        IUnitOfWork unitOfWork,
     IWriteRepository<Gasto, GastoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// 🔥 OVERRIDE: Cargamos la entidad real para poder disparar el evento de dominio.
    /// </summary>
    protected override async Task<Gasto?> LoadEntityForDeletionAsync(Guid id, CancellationToken cancellationToken)
    {
        // 1. Cargar la entidad real desde la base de datos
        var gasto = await _writeRepository.GetByIdAsync(id, cancellationToken);

        if (gasto == null)
        {
            return null;
        }

        // 2. 🔥 Marcar como eliminado y disparar evento de dominio
        gasto.MarkAsDeleted();

        return gasto;
    }
}


