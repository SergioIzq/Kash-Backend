using AhorroLand.Domain;
using AhorroLand.Domain.Traspasos.Eventos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Infrastructure.EventHandlers;

/// <summary>
/// Handler que actualiza los saldos de las cuentas cuando se crea un traspaso.
/// El traspaso RESTA de la cuenta origen y SUMA a la cuenta destino.
/// </summary>
public sealed class TraspasoCreadoEventHandler : INotificationHandler<TraspasoCreadoEvent>
{
    private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TraspasoCreadoEventHandler> _logger;

    public TraspasoCreadoEventHandler(
        IWriteRepository<Cuenta, CuentaId> cuentaRepository,
     IUnitOfWork unitOfWork,
     ILogger<TraspasoCreadoEventHandler> logger)
 {
        _cuentaRepository = cuentaRepository;
     _unitOfWork = unitOfWork;
  _logger = logger;
    }

    public async Task Handle(TraspasoCreadoEvent notification, CancellationToken cancellationToken)
    {
    try
      {
            // 1. Obtener cuenta origen
  var cuentaOrigen = await _cuentaRepository.GetByIdAsync(notification.CuentaOrigenId.Value, cancellationToken);

      if (cuentaOrigen == null)
  {
     _logger.LogWarning(
        "No se encontró la cuenta origen {CuentaOrigenId} para traspaso {TraspasoId}",
   notification.CuentaOrigenId,
  notification.TraspasoId);
      return;
   }

     // 2. Obtener cuenta destino
            var cuentaDestino = await _cuentaRepository.GetByIdAsync(notification.CuentaDestinoId.Value, cancellationToken);

   if (cuentaDestino == null)
         {
  _logger.LogWarning(
         "No se encontró la cuenta destino {CuentaDestinoId} para traspaso {TraspasoId}",
      notification.CuentaDestinoId,
      notification.TraspasoId);
  return;
  }

      // 3. Retirar de cuenta origen
      cuentaOrigen.Retirar(notification.Importe);
         _cuentaRepository.Update(cuentaOrigen);

   // 4. Depositar en cuenta destino
     cuentaDestino.Depositar(notification.Importe);
       _cuentaRepository.Update(cuentaDestino);

    // 5. Guardar cambios
         await _unitOfWork.SaveChangesAsync(cancellationToken);

  _logger.LogInformation(
    "Saldo actualizado por traspaso {TraspasoId}: Cuenta origen {CuentaOrigen} -{Importe} ? Cuenta destino {CuentaDestino} +{Importe}",
     notification.TraspasoId,
  notification.CuentaOrigenId,
  notification.Importe.Valor,
     notification.CuentaDestinoId,
     notification.Importe.Valor);
  }
        catch (InvalidOperationException ex)
        {
   // Saldo insuficiente en cuenta origen
 _logger.LogError(ex,
     "Saldo insuficiente en cuenta origen {CuentaOrigenId} para traspaso {TraspasoId} por {Importe}",
      notification.CuentaOrigenId,
    notification.TraspasoId,
    notification.Importe.Valor);
         throw;
   }
      catch (Exception ex)
      {
          _logger.LogError(ex,
            "Error al actualizar saldos por traspaso {TraspasoId}",
         notification.TraspasoId);
   throw;
  }
    }
}
