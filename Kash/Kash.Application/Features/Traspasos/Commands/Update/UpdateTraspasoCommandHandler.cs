using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Traspasos.Commands;

public sealed class UpdateTraspasoCommandHandler
    : AbsUpdateCommandHandler<Traspaso, TraspasoId, TraspasoDto, UpdateTraspasoCommand>
{
    public UpdateTraspasoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Traspaso, TraspasoId> writeRepository,
   ICacheService cacheService,
        IReadRepository<Traspaso, TraspasoDto, TraspasoId> readOnlyRepository,
        IUserContext userContext
        )
   : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override void ApplyChanges(Traspaso entity, UpdateTraspasoCommand command, Dictionary<string, object>? dependencies = null)
    {
        // Crear Value Objects desde el command
        var cuentaOrigenId = CuentaId.Create(command.CuentaOrigenId).Value;
        var cuentaDestinoId = CuentaId.Create(command.CuentaDestinoId).Value;
        var importe = Cantidad.Create(command.Importe).Value;
        var fecha = FechaRegistro.Create(command.FechaEjecucion).Value;
        var descripcion = new Descripcion(command.Descripcion);

        // Llamar al método Update de la entidad que dispara el evento
        entity.Update(
      cuentaOrigenId,
        cuentaDestinoId,
          importe,
            fecha,
         descripcion,
         command.Activo);
    }
}


