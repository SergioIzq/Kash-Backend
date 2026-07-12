using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.TraspasosProgramados.Commands;

public sealed class UpdateTraspasoProgramadoCommandHandler
    : AbsUpdateCommandHandler<TraspasoProgramado, TraspasoProgramadoId, TraspasoProgramadoDto, UpdateTraspasoProgramadoCommand>
{
    public UpdateTraspasoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<TraspasoProgramado, TraspasoProgramadoId> writeRepository,
        ICacheService cacheService,
        IReadRepository<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId> readOnlyRepository,
        IUserContext userContext
    )
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override void ApplyChanges(TraspasoProgramado entity, UpdateTraspasoProgramadoCommand command, Dictionary<string, object>? dependencies = null)
    {
        // Crear Value Objects desde el command
        var cuentaOrigenId = CuentaId.Create(command.CuentaOrigenId).Value;
        var cuentaDestinoId = CuentaId.Create(command.CuentaDestinoId).Value;
        var importe = Cantidad.Create(command.Importe).Value;
        var frecuencia = Frecuencia.Create(command.Frecuencia).Value;
        var descripcion = string.IsNullOrEmpty(command.Descripcion)
            ? (Descripcion?)null
            : new Descripcion(command.Descripcion);

        // Llamar al método Update de la entidad que dispara el evento
        var result = entity.Update(
            cuentaOrigenId,
            cuentaDestinoId,
            importe,
            command.FechaEjecucion,
            frecuencia,
            command.Activo,
            descripcion);

        // Si hay error, lanzar excepción para que AbsUpdateCommandHandler lo capture
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}


