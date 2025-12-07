using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.IngresosProgramados.Commands;

public sealed class UpdateIngresoProgramadoCommandHandler
    : AbsUpdateCommandHandler<IngresoProgramado, IngresoProgramadoId, IngresoProgramadoDto, UpdateIngresoProgramadoCommand>
{
    public UpdateIngresoProgramadoCommandHandler(
  IUnitOfWork unitOfWork,
   IWriteRepository<IngresoProgramado, IngresoProgramadoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext
        )
     : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override void ApplyChanges(IngresoProgramado entity, UpdateIngresoProgramadoCommand command)
    {
        // Nota: Si la entidad IngresoProgramado no tiene un método Update,
        // esta implementación debería agregarse al modelo de dominio.
        throw new NotSupportedException("La actualización de ingresos programados requiere implementación en el modelo de dominio.");
    }
}

