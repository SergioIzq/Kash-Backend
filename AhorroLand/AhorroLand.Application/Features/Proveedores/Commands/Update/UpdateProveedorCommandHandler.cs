using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Proveedores.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Proveedor.
/// </summary>
public sealed class UpdateProveedorCommandHandler
    : AbsUpdateCommandHandler<Proveedor, ProveedorId, ProveedorDto, UpdateProveedorCommand>
{
    public UpdateProveedorCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Proveedor, ProveedorId> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<Proveedor, ProveedorDto, ProveedorId> readOnlyRepository,
        IUserContext userContext
        )
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override void ApplyChanges(Proveedor entity, UpdateProveedorCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = Nombre.Create(command.Nombre).Value;

        entity.Update(
            nuevoNombreVO
        );
    }
}
