using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Traspasos.Commands;

public sealed class DeleteTraspasoCommandHandler
    : DeleteCommandHandler<Traspaso, TraspasoId, DeleteTraspasoCommand>
{
    public DeleteTraspasoCommandHandler(
        IUnitOfWork unitOfWork,
  IWriteRepository<Traspaso, TraspasoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext
        )
      : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}



