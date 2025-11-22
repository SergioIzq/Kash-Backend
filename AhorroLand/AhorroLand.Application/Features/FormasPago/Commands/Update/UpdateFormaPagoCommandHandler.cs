using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.FormasPago.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad FormaPago.
/// </summary>
public sealed class UpdateFormaPagoCommandHandler
    : AbsUpdateCommandHandler<FormaPago, FormaPagoDto, UpdateFormaPagoCommand>
{
    public UpdateFormaPagoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<FormaPago> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<FormaPago, FormaPagoDto> readOnlyRepository
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override void ApplyChanges(FormaPago entity, UpdateFormaPagoCommand command)
    {
        // 1. Crear el Value Object 'Nombre' a partir del string del comando.
        // Esto automáticamente ejecuta las reglas de validación del nombre.
        var nuevoNombreVO = new Nombre(command.Nombre);

        entity.Update(
            nuevoNombreVO
        );
    }
}
