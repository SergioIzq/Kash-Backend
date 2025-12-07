using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Traspasos.Commands;

public sealed class UpdateTraspasoCommandHandler
    : AbsUpdateCommandHandler<Traspaso, TraspasoId, TraspasoDto, UpdateTraspasoCommand>
{
    public UpdateTraspasoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Traspaso, TraspasoId> writeRepository,
   ICacheService cacheService,
        IReadRepositoryWithDto<Traspaso, TraspasoDto, TraspasoId> readOnlyRepository,
        IUserContext userContext
        )
        : base(unitOfWork, writeRepository, cacheService, userContext)
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

