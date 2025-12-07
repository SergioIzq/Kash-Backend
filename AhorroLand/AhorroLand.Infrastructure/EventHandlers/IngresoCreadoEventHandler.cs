using AhorroLand.Domain;
using AhorroLand.Domain.Ingresos.Eventos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Infrastructure.EventHandlers;

/// <summary>
/// Handler que actualiza el saldo de la cuenta cuando se crea un ingreso.
/// El ingreso SUMA al saldo de la cuenta.
/// </summary>
public sealed class IngresoCreadoEventHandler : INotificationHandler<IngresoCreadoEvent>
{
    private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IngresoCreadoEventHandler> _logger;

    public IngresoCreadoEventHandler(
        IWriteRepository<Cuenta, CuentaId> cuentaRepository,
        IUnitOfWork unitOfWork,
    ILogger<IngresoCreadoEventHandler> logger)
    {
     _cuentaRepository = cuentaRepository;
   _unitOfWork = unitOfWork;
_logger = logger;
    }

  public async Task Handle(IngresoCreadoEvent notification, CancellationToken cancellationToken)
    {
        try
     {
   // 1. Obtener la cuenta
        var cuenta = await _cuentaRepository.GetByIdAsync(notification.CuentaId.Value, cancellationToken);

     if (cuenta == null)
   {
       _logger.LogWarning(
   "No se encontró la cuenta {CuentaId} para actualizar saldo por ingreso {IngresoId}",
       notification.CuentaId,
  notification.IngresoId);
    return;
      }

     // 2. Depositar el importe del ingreso (aumentar saldo)
       cuenta.Depositar(notification.Importe);

    // 3. Marcar como modificado y guardar
   _cuentaRepository.Update(cuenta);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
     "Saldo actualizado: Cuenta {CuentaId} + {Importe} por ingreso {IngresoId}",
      notification.CuentaId,
       notification.Importe.Valor,
    notification.IngresoId);
   }
      catch (Exception ex)
        {
            _logger.LogError(ex,
     "Error al actualizar saldo de cuenta {CuentaId} por ingreso {IngresoId}",
   notification.CuentaId,
   notification.IngresoId);
       throw;
        }
}
}
