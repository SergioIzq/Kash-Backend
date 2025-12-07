using AhorroLand.Domain;
using AhorroLand.Domain.Ingresos.Eventos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Infrastructure.EventHandlers;

/// <summary>
/// Handler que revierte el efecto en el saldo de la cuenta cuando se elimina un ingreso.
/// El ingreso se había sumado, ahora lo restamos (retiramos).
/// </summary>
public sealed class IngresoEliminadoEventHandler : INotificationHandler<IngresoEliminadoEvent>
{
  private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<IngresoEliminadoEventHandler> _logger;

    public IngresoEliminadoEventHandler(
        IWriteRepository<Cuenta, CuentaId> cuentaRepository,
        IUnitOfWork unitOfWork,
  ILogger<IngresoEliminadoEventHandler> logger)
    {
        _cuentaRepository = cuentaRepository;
     _unitOfWork = unitOfWork;
    _logger = logger;
    }

    public async Task Handle(IngresoEliminadoEvent notification, CancellationToken cancellationToken)
    {
    try
        {
    // 1. Obtener la cuenta
   var cuenta = await _cuentaRepository.GetByIdAsync(notification.CuentaId.Value, cancellationToken);

if (cuenta == null)
       {
  _logger.LogWarning(
        "No se encontró la cuenta {CuentaId} para revertir ingreso eliminado {IngresoId}",
     notification.CuentaId,
   notification.IngresoId);
     return;
            }

// 2. Retirar el importe (revertir el depósito que se hizo al crear el ingreso)
            cuenta.Retirar(notification.Importe);

       // 3. Marcar como modificado y guardar
_cuentaRepository.Update(cuenta);
       await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
  "Saldo revertido: Cuenta {CuentaId} - {Importe} por eliminación de ingreso {IngresoId}",
      notification.CuentaId,
     notification.Importe.Valor,
  notification.IngresoId);
        }
        catch (InvalidOperationException ex)
  {
    _logger.LogError(ex,
    "Saldo insuficiente al eliminar ingreso {IngresoId}",
  notification.IngresoId);
 throw;
     }
        catch (Exception ex)
      {
   _logger.LogError(ex,
     "Error al revertir saldo de cuenta {CuentaId} por eliminación de ingreso {IngresoId}",
   notification.CuentaId,
 notification.IngresoId);
  throw;
   }
    }
}
