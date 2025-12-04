using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Cuentas.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Cuenta.
/// </summary>
public sealed class UpdateCuentaCommandHandler
    : AbsUpdateCommandHandler<Cuenta, CuentaId, CuentaDto, UpdateCuentaCommand>
{
    public UpdateCuentaCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Cuenta, CuentaId> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<Cuenta, CuentaDto, CuentaId> readOnlyRepository
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override void ApplyChanges(Cuenta entity, UpdateCuentaCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = Nombre.Create(command.Nombre).Value;

        entity.Update(
            nuevoNombreVO
        );
    }
}
