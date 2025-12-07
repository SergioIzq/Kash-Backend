using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.IngresosProgramados.Commands;

public sealed class DeleteIngresoProgramadoCommandHandler
    : DeleteCommandHandler<IngresoProgramado, IngresoProgramadoId, DeleteIngresoProgramadoCommand>
{
    public DeleteIngresoProgramadoCommandHandler(
      IUnitOfWork unitOfWork,
        IWriteRepository<IngresoProgramado, IngresoProgramadoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}



