using AhorroLand.Domain;
using AhorroLand.Domain.Gastos.Eventos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Infrastructure.EventHandlers;

/// <summary>
/// Handler que actualiza el saldo de las cuentas cuando se actualiza un gasto.
/// Revierte el efecto en la cuenta anterior y aplica el nuevo efecto en la cuenta nueva.
/// </summary>
public sealed class GastoActualizadoEventHandler : INotificationHandler<GastoActualizadoEvent>
{
    private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;
 private readonly ILogger<GastoActualizadoEventHandler> _logger;

  public GastoActualizadoEventHandler(
        IWriteRepository<Cuenta, CuentaId> cuentaRepository,
        IUnitOfWork unitOfWork,
        ILogger<GastoActualizadoEventHandler> logger)
    {
        _cuentaRepository = cuentaRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(GastoActualizadoEvent notification, CancellationToken cancellationToken)
    {
        try
 {
            // Si la cuenta no cambió y el importe tampoco, no hacemos nada
     if (notification.CuentaIdAnterior.Equals(notification.CuentaIdNueva) && 
                notification.ImporteAnterior.Equals(notification.ImporteNuevo))
       {
    return;
}

            // CASO 1: Cambió la cuenta (movimiento entre cuentas)
  if (!notification.CuentaIdAnterior.Equals(notification.CuentaIdNueva))
            {
        // 1.1 Revertir en cuenta anterior (depositar lo que se había retirado)
     var cuentaAnterior = await _cuentaRepository.GetByIdAsync(notification.CuentaIdAnterior.Value, cancellationToken);
          if (cuentaAnterior != null)
    {
       cuentaAnterior.Depositar(notification.ImporteAnterior);
   _cuentaRepository.Update(cuentaAnterior);
                }

        // 1.2 Aplicar en cuenta nueva (retirar nuevo importe)
            var cuentaNueva = await _cuentaRepository.GetByIdAsync(notification.CuentaIdNueva.Value, cancellationToken);
if (cuentaNueva != null)
          {
            cuentaNueva.Retirar(notification.ImporteNuevo);
   _cuentaRepository.Update(cuentaNueva);
     }
  }
       // CASO 2: Misma cuenta, pero cambió el importe
    else
 {
     var cuenta = await _cuentaRepository.GetByIdAsync(notification.CuentaIdNueva.Value, cancellationToken);
      if (cuenta != null)
                {
        // Revertir importe anterior
         cuenta.Depositar(notification.ImporteAnterior);
             // Aplicar nuevo importe
            cuenta.Retirar(notification.ImporteNuevo);
      _cuentaRepository.Update(cuenta);
       }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
           "Saldo actualizado por modificación de gasto {GastoId}: Cuenta anterior {CuentaAnterior} ({ImporteAnterior}) -> Cuenta nueva {CuentaNueva} ({ImporteNuevo})",
 notification.GastoId,
            notification.CuentaIdAnterior,
    notification.ImporteAnterior.Valor,
      notification.CuentaIdNueva,
 notification.ImporteNuevo.Valor);
    }
        catch (InvalidOperationException ex)
      {
  _logger.LogError(ex,
    "Saldo insuficiente al actualizar gasto {GastoId}",
    notification.GastoId);
       throw;
        }
      catch (Exception ex)
    {
            _logger.LogError(ex,
     "Error al actualizar saldo por modificación de gasto {GastoId}",
              notification.GastoId);
    throw;
        }
    }
}
