using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
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
        // Crear Value Objects desde el command
        var cuentaOrigenId = CuentaId.Create(command.CuentaOrigenId).Value;
        var cuentaDestinoId = CuentaId.Create(command.CuentaDestinoId).Value;
        var importe = Cantidad.Create(command.Importe).Value;
        var fecha = FechaRegistro.Create(command.FechaEjecucion).Value;
        var descripcion = new Descripcion(command.Descripcion);

        // 🔥 Llamar al método Update de la entidad que dispara el evento
        entity.Update(
      cuentaOrigenId,
        cuentaDestinoId,
          importe,
            fecha,
         descripcion,
         command.Activo);
    }
}

