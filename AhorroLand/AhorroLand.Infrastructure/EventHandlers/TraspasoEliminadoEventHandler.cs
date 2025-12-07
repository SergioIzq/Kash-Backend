using AhorroLand.Domain;
using AhorroLand.Domain.Traspasos.Eventos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Infrastructure.EventHandlers;

/// <summary>
/// Handler que revierte el efecto en los saldos de las cuentas cuando se elimina un traspaso.
/// Devuelve el importe a la cuenta origen y lo retira de la cuenta destino.
/// </summary>
public sealed class TraspasoEliminadoEventHandler : INotificationHandler<TraspasoEliminadoEvent>
{
    private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;
 private readonly ILogger<TraspasoEliminadoEventHandler> _logger;

    public TraspasoEliminadoEventHandler(
        IWriteRepository<Cuenta, CuentaId> cuentaRepository,
        IUnitOfWork unitOfWork,
        ILogger<TraspasoEliminadoEventHandler> logger)
 {
   _cuentaRepository = cuentaRepository;
        _unitOfWork = unitOfWork;
  _logger = logger;
    }

    public async Task Handle(TraspasoEliminadoEvent notification, CancellationToken cancellationToken)
    {
   try
        {
      // 1. Obtener cuenta origen y devolver el importe (depositar)
      var cuentaOrigen = await _cuentaRepository.GetByIdAsync(
    notification.CuentaOrigenId.Value, cancellationToken);

   if (cuentaOrigen == null)
      {
 _logger.LogWarning(
"No se encontró la cuenta origen {CuentaOrigenId} para revertir traspaso eliminado {TraspasoId}",
 notification.CuentaOrigenId,
  notification.TraspasoId);
  }
   else
            {
   cuentaOrigen.Depositar(notification.Importe);
     _cuentaRepository.Update(cuentaOrigen);
   }

          // 2. Obtener cuenta destino y retirar el importe
   var cuentaDestino = await _cuentaRepository.GetByIdAsync(
     notification.CuentaDestinoId.Value, cancellationToken);

   if (cuentaDestino == null)
       {
    _logger.LogWarning(
       "No se encontró la cuenta destino {CuentaDestinoId} para revertir traspaso eliminado {TraspasoId}",
     notification.CuentaDestinoId,
       notification.TraspasoId);
    }
            else
   {
       cuentaDestino.Retirar(notification.Importe);
 _cuentaRepository.Update(cuentaDestino);
   }

    // 3. Guardar cambios
    await _unitOfWork.SaveChangesAsync(cancellationToken);

     _logger.LogInformation(
     "Saldo revertido por eliminación de traspaso {TraspasoId}: " +
"Cuenta origen {CuentaOrigen} +{Importe} ? Cuenta destino {CuentaDestino} -{Importe}",
      notification.TraspasoId,
  notification.CuentaOrigenId,
    notification.Importe.Valor,
     notification.CuentaDestinoId,
    notification.Importe.Valor);
        }
        catch (InvalidOperationException ex)
      {
     _logger.LogError(ex,
     "Saldo insuficiente al eliminar traspaso {TraspasoId}",
   notification.TraspasoId);
   throw;
     }
 catch (Exception ex)
        {
   _logger.LogError(ex,
       "Error al revertir saldos por eliminación de traspaso {TraspasoId}",
    notification.TraspasoId);
            throw;
     }
 }
}
