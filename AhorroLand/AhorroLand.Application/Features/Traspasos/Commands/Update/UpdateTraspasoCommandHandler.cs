using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Traspasos.Commands;

public sealed class UpdateTraspasoCommandHandler
    : AbsUpdateCommandHandler<Traspaso, TraspasoDto, UpdateTraspasoCommand>
{
    public UpdateTraspasoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Traspaso> writeRepository,
   ICacheService cacheService,
        IReadRepositoryWithDto<Traspaso, TraspasoDto> readOnlyRepository
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override void ApplyChanges(Traspaso entity, UpdateTraspasoCommand command)
    {
        // Nota: Traspaso tiene propiedades readonly, por lo que esta operación
        // podría requerir recrear la entidad o usar reflexión.
        // Por ahora, este handler existe para completitud de la API pero
        // la entidad de dominio debería implementar un método Update si se requiere.
        throw new NotSupportedException("La actualización de traspasos no está soportada por el modelo de dominio actual.");
    }
}

