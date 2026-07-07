using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Ingresos.Commands;

/// <summary>
/// Manejador concreto para eliminar un Ingreso.
/// Sobrescribe LoadEntityForDeletionAsync para cargar la entidad y disparar el evento MarkAsDeleted.
/// </summary>
public sealed class DeleteIngresoCommandHandler
    : DeleteCommandHandler<Ingreso, IngresoId, DeleteIngresoCommand>
{
    public DeleteIngresoCommandHandler(
      IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso, IngresoId> writeRepository,
      ICacheService cacheService,
      IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// OVERRIDE: Cargamos la entidad real para poder disparar el evento de dominio.
    /// </summary>
    protected override async Task<Ingreso?> LoadEntityForDeletionAsync(Guid id, CancellationToken cancellationToken)
    {
        // 1. Cargar la entidad real desde la base de datos
        var ingreso = await _writeRepository.GetByIdAsync(id, cancellationToken);

        if (ingreso == null)
        {
            return null;
        }

        // 2. Marcar como eliminado y disparar evento de dominio
        ingreso.MarkAsDeleted();

        return ingreso;
    }
}


