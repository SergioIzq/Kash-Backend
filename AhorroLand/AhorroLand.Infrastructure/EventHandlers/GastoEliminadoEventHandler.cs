using AhorroLand.Domain;
using AhorroLand.Domain.Gastos.Eventos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Infrastructure.EventHandlers;

/// <summary>
/// Handler que revierte el efecto en el saldo de la cuenta cuando se elimina un gasto.
/// El gasto se había restado, ahora lo devolvemos (depositamos).
/// </summary>
public sealed class GastoEliminadoEventHandler : INotificationHandler<GastoEliminadoEvent>
{
    private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GastoEliminadoEventHandler> _logger;

    public GastoEliminadoEventHandler(
    IWriteRepository<Cuenta, CuentaId> cuentaRepository,
        IUnitOfWork unitOfWork,
      ILogger<GastoEliminadoEventHandler> logger)
    {
        _cuentaRepository = cuentaRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(GastoEliminadoEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Obtener la cuenta
            var cuenta = await _cuentaRepository.GetByIdAsync(notification.CuentaId.Value, cancellationToken);

            if (cuenta == null)
            {
                _logger.LogWarning(
                  "No se encontró la cuenta {CuentaId} para revertir gasto eliminado {GastoId}",
           notification.CuentaId,
              notification.GastoId);
                return;
            }

            // 2. Depositar el importe (revertir el retiro que se hizo al crear el gasto)
            cuenta.Depositar(notification.Importe);

            // 3. Marcar como modificado y guardar
            _cuentaRepository.Update(cuenta);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
     "Saldo revertido: Cuenta {CuentaId} + {Importe} por eliminación de gasto {GastoId}",
 notification.CuentaId,
       notification.Importe.Valor,
    notification.GastoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
       "Error al revertir saldo de cuenta {CuentaId} por eliminación de gasto {GastoId}",
               notification.CuentaId,
        notification.GastoId);
            throw;
        }
    }
}
