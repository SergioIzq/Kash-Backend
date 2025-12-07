using AhorroLand.Domain;
using AhorroLand.Domain.Traspasos.Eventos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Infrastructure.EventHandlers;

/// <summary>
/// Handler que actualiza los saldos de las cuentas cuando se actualiza un traspaso.
/// Revierte el efecto en las cuentas anteriores y aplica el nuevo efecto en las cuentas nuevas.
/// </summary>
public sealed class TraspasoActualizadoEventHandler : INotificationHandler<TraspasoActualizadoEvent>
{
    private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TraspasoActualizadoEventHandler> _logger;

  public TraspasoActualizadoEventHandler(
  IWriteRepository<Cuenta, CuentaId> cuentaRepository,
        IUnitOfWork unitOfWork,
        ILogger<TraspasoActualizadoEventHandler> logger)
  {
    _cuentaRepository = cuentaRepository;
   _unitOfWork = unitOfWork;
   _logger = logger;
    }

    public async Task Handle(TraspasoActualizadoEvent notification, CancellationToken cancellationToken)
 {
        try
   {
            // CASO 1: Revertir operación anterior
     // 1.1 Cuenta origen anterior: devolver el importe (depositar)
        var cuentaOrigenAnterior = await _cuentaRepository.GetByIdAsync(
       notification.CuentaOrigenIdAnterior.Value, cancellationToken);

    if (cuentaOrigenAnterior != null)
  {
      cuentaOrigenAnterior.Depositar(notification.ImporteAnterior);
  _cuentaRepository.Update(cuentaOrigenAnterior);
         }

   // 1.2 Cuenta destino anterior: retirar el importe
     var cuentaDestinoAnterior = await _cuentaRepository.GetByIdAsync(
  notification.CuentaDestinoIdAnterior.Value, cancellationToken);

   if (cuentaDestinoAnterior != null)
     {
              cuentaDestinoAnterior.Retirar(notification.ImporteAnterior);
   _cuentaRepository.Update(cuentaDestinoAnterior);
         }

       // CASO 2: Aplicar nueva operación
  // 2.1 Cuenta origen nueva: retirar el nuevo importe
     var cuentaOrigenNueva = await _cuentaRepository.GetByIdAsync(
   notification.CuentaOrigenIdNueva.Value, cancellationToken);

   if (cuentaOrigenNueva != null)
     {
     cuentaOrigenNueva.Retirar(notification.ImporteNuevo);
_cuentaRepository.Update(cuentaOrigenNueva);
            }

  // 2.2 Cuenta destino nueva: depositar el nuevo importe
  var cuentaDestinoNueva = await _cuentaRepository.GetByIdAsync(
           notification.CuentaDestinoIdNueva.Value, cancellationToken);

       if (cuentaDestinoNueva != null)
      {
       cuentaDestinoNueva.Depositar(notification.ImporteNuevo);
      _cuentaRepository.Update(cuentaDestinoNueva);
  }

   // 3. Guardar todos los cambios
    await _unitOfWork.SaveChangesAsync(cancellationToken);

     _logger.LogInformation(
         "Saldo actualizado por modificación de traspaso {TraspasoId}: " +
           "Anterior [{CuentaOrigenAnt} -{ImporteAnt} ? {CuentaDestinoAnt} +{ImporteAnt}] " +
 "? Nuevo [{CuentaOrigenNueva} -{ImporteNuevo} ? {CuentaDestinoNueva} +{ImporteNuevo}]",
    notification.TraspasoId,
notification.CuentaOrigenIdAnterior,
         notification.ImporteAnterior.Valor,
      notification.CuentaDestinoIdAnterior,
  notification.ImporteAnterior.Valor,
 notification.CuentaOrigenIdNueva,
       notification.ImporteNuevo.Valor,
    notification.CuentaDestinoIdNueva,
       notification.ImporteNuevo.Valor);
        }
        catch (InvalidOperationException ex)
        {
     _logger.LogError(ex,
   "Saldo insuficiente al actualizar traspaso {TraspasoId}",
 notification.TraspasoId);
 throw;
        }
 catch (Exception ex)
        {
  _logger.LogError(ex,
      "Error al actualizar saldos por modificación de traspaso {TraspasoId}",
         notification.TraspasoId);
   throw;
        }
    }
}
