using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.GastosProgramados.Commands;

public sealed class DeleteGastoProgramadoCommandHandler
    : DeleteCommandHandler<GastoProgramado, GastoProgramadoId, DeleteGastoProgramadoCommand>
{
    public DeleteGastoProgramadoCommandHandler(
     IUnitOfWork unitOfWork,
   IWriteRepository<GastoProgramado, GastoProgramadoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
  : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }
}



