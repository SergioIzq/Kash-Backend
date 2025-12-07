using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Proveedores.Commands;

/// <summary>
/// Manejador concreto para eliminar una Proveedor.
/// Hereda toda la lógica de la clase base genérica.
/// </summary>
public sealed class DeleteProveedorCommandHandler
    : DeleteCommandHandler<Proveedor, ProveedorId, DeleteProveedorCommand>
{
    public DeleteProveedorCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Proveedor, ProveedorId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}


