using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Cuentas.Commands;

public sealed class CreateCuentaCommandHandler : AbsCreateCommandHandler<Cuenta, CuentaId, CreateCuentaCommand>
{
    public CreateCuentaCommandHandler(
    IUnitOfWork unitOfWork,
    IWriteRepository<Cuenta, CuentaId> writeRepository,
    ICacheService cacheService,
    IUserContext userContext)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override Cuenta CreateEntity(CreateCuentaCommand command)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var saldoVO = Cantidad.Create(command.Saldo).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        var newCuenta = Cuenta.Create(nombreVO, saldoVO, usuarioId);
        return newCuenta;
    }
}

