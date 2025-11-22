using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Proveedores.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Proveedor.
/// </summary>
public sealed class UpdateProveedorCommandHandler
    : AbsUpdateCommandHandler<Proveedor, ProveedorDto, UpdateProveedorCommand>
{
    public UpdateProveedorCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Proveedor> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<Proveedor, ProveedorDto> readOnlyRepository
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override void ApplyChanges(Proveedor entity, UpdateProveedorCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = new Nombre(command.Nombre);

        entity.Update(
            nuevoNombreVO
        );
    }
}
