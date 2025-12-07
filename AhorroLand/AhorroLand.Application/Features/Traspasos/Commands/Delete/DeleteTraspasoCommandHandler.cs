using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Traspasos.Commands;

/// <summary>
/// Manejador concreto para eliminar un Traspaso.
/// 🔥 Sobrescribe LoadEntityForDeletionAsync para cargar la entidad y disparar el evento MarkAsDeleted.
/// </summary>
public sealed class DeleteTraspasoCommandHandler
    : DeleteCommandHandler<Traspaso, TraspasoId, DeleteTraspasoCommand>
{
    public DeleteTraspasoCommandHandler(
        IUnitOfWork unitOfWork,
  IWriteRepository<Traspaso, TraspasoId> writeRepository,
    ICacheService cacheService,
        IUserContext userContext
        )
      : base(unitOfWork, writeRepository, cacheService, userContext)
    {
  }

    /// <summary>
    /// 🔥 OVERRIDE: Cargamos la entidad real para poder disparar el evento de dominio.
    /// </summary>
    protected override async Task<Traspaso?> LoadEntityForDeletionAsync(Guid id, CancellationToken cancellationToken)
    {
    // 1. Cargar la entidad real desde la base de datos
     var traspaso = await _writeRepository.GetByIdAsync(id, cancellationToken);

 if (traspaso == null)
        {
            return null;
        }

        // 2. 🔥 Marcar como eliminado y disparar evento de dominio
        traspaso.MarkAsDeleted();

        return traspaso;
    }
}



