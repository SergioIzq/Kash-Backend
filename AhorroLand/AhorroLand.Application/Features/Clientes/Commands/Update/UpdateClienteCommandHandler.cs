using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Clientes.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Categoria.
/// </summary>
public sealed class UpdateClienteCommandHandler
    : AbsUpdateCommandHandler<Cliente, ClienteDto, UpdateClienteCommand>
{
    public UpdateClienteCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Cliente> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<Cliente, ClienteDto> readOnlyRepository
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override void ApplyChanges(Cliente entity, UpdateClienteCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = new Nombre(command.Nombre);

        entity.Update(
            nuevoNombreVO
        );
    }
}
