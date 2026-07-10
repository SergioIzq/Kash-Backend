using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.TraspasosProgramados.Commands;

/// <summary>
/// Manejador concreto para eliminar un TraspasoProgramado.
/// Sobrescribe LoadEntityForDeletionAsync para cargar la entidad y disparar el evento MarkAsDeleted.
/// </summary>
public sealed class DeleteTraspasoProgramadoCommandHandler
    : DeleteCommandHandler<TraspasoProgramado, TraspasoProgramadoId, DeleteTraspasoProgramadoCommand>
{
    public DeleteTraspasoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
  IWriteRepository<TraspasoProgramado, TraspasoProgramadoId> writeRepository,
    ICacheService cacheService,
        IUserContext userContext
        )
      : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// OVERRIDE: Cargamos la entidad real para poder disparar el evento de dominio.
    /// </summary>
    protected override async Task<TraspasoProgramado?> LoadEntityForDeletionAsync(Guid id, CancellationToken cancellationToken)
    {
        // 1. Cargar la entidad real desde la base de datos
        var traspaso = await _writeRepository.GetByIdAsync(id, cancellationToken);

        if (traspaso == null)
        {
            return null;
        }

        // 2. Marcar como eliminado y disparar evento de dominio
        traspaso.MarkAsDeleted();

        return traspaso;
    }
}



