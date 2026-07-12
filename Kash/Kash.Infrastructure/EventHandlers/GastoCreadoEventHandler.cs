using Kash.Domain;
using Kash.Domain.Gastos.Eventos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Infrastructure.EventHandlers;

/// <summary>
/// Handler que actualiza el saldo de la cuenta cuando se crea un gasto.
/// El gasto RESTA del saldo de la cuenta.
/// </summary>
public sealed class GastoCreadoEventHandler : INotificationHandler<GastoCreadoEvent>
{
    private readonly IWriteRepository<Cuenta, CuentaId> _cuentaRepository;
    private readonly ILogger<GastoCreadoEventHandler> _logger;

    public GastoCreadoEventHandler(
        IWriteRepository<Cuenta, CuentaId> cuentaRepository,
        ILogger<GastoCreadoEventHandler> logger)
    {
        _cuentaRepository = cuentaRepository;
        _logger = logger;
    }

    public async Task Handle(GastoCreadoEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Obtener la cuenta
            var cuenta = await _cuentaRepository.GetByIdAsync(notification.CuentaId.Value, cancellationToken);

            if (cuenta == null)
            {
                _logger.LogWarning(
            "No se encontró la cuenta {CuentaId} para actualizar saldo por gasto {GastoId}",
     notification.CuentaId,
    notification.GastoId);
                return;
            }

            // 2. Retirar el importe del gasto (disminuir saldo)
            cuenta.Retirar(notification.Importe);

            // 3. Marcar como modificado y guardar
            _cuentaRepository.Update(cuenta);

            _logger.LogInformation(
   "Saldo actualizado: Cuenta {CuentaId} - Gasto {GastoId} por {Importe}",
       notification.CuentaId,
              notification.GastoId,
         notification.Importe.Valor);
        }
        catch (InvalidOperationException ex)
        {
            // Saldo insuficiente
            _logger.LogError(ex,
          "Saldo insuficiente en cuenta {CuentaId} para gasto {GastoId} por {Importe}",
              notification.CuentaId,
              notification.GastoId,
        notification.Importe.Valor);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
        "Error al actualizar saldo de cuenta {CuentaId} por gasto {GastoId}",
   notification.CuentaId,
       notification.GastoId);
            throw;
        }
    }
}
