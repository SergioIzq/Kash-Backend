using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.TraspasosProgramados.Commands;

public sealed class UpdateTraspasoProgramadoCommandHandler
    : AbsUpdateCommandHandler<TraspasoProgramado, TraspasoProgramadoId, TraspasoProgramadoDto, UpdateTraspasoProgramadoCommand>
{
    public UpdateTraspasoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<TraspasoProgramado, TraspasoProgramadoId> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId> readOnlyRepository,
        IUserContext userContext
    )
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override void ApplyChanges(TraspasoProgramado entity, UpdateTraspasoProgramadoCommand command)
    {
        // Nota: Si la entidad TraspasoProgramado tiene un método Reprogramar,
        // debería usarse aquí. De lo contrario, esta implementación debe agregarse.
        throw new NotSupportedException("La actualización de traspasos programados requiere implementación en el modelo de dominio.");
    }
}

