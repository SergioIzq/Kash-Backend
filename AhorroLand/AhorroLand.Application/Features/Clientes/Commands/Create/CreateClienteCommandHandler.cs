using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Clientes.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Cliente.
/// </summary>
public sealed class CreateClienteCommandHandler
    : AbsCreateCommandHandler<Cliente, ClienteId, CreateClienteCommand>
{
    public CreateClienteCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Cliente, ClienteId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// **Implementación de la lógica de negocio**: Crea la entidad Cliente.
    /// Este es el único método que tienes que implementar y donde se aplica el DDD.
    /// </summary>
    /// <param name="command">El comando con los datos de creación.</param>
    /// <returns>La nueva entidad Cliente creada.</returns>
    protected override Cliente CreateEntity(CreateClienteCommand command)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var usuarioIdVO = UsuarioId.Create(command.UsuarioId).Value;

        var newCliente = Cliente.Create(nombreVO, usuarioIdVO);

        return newCliente;
    }
}
