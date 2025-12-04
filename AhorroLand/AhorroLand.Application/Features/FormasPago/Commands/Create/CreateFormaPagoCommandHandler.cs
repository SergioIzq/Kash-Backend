using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.FormasPago.Commands;

public sealed class CreateFormaPagoCommandHandler : AbsCreateCommandHandler<FormaPago, FormaPagoId, CreateFormaPagoCommand>
{
    public CreateFormaPagoCommandHandler(
    IUnitOfWork unitOfWork,
    IWriteRepository<FormaPago, FormaPagoId> writeRepository,
    ICacheService cacheService)
    : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override FormaPago CreateEntity(CreateFormaPagoCommand command)
    {
        var nombreVO = Nombre.Create(command.Nombre).Value;
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        var newFormaPago = FormaPago.Create(nombreVO, usuarioId);
        return newFormaPago;
    }
}

