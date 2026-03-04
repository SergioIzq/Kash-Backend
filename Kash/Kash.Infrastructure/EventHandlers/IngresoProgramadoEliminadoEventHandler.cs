using Kash.Domain.IngresosProgramados.Eventos;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Infrastructure.EventHandlers;

/// <summary>
/// Handler de infraestructura que escucha el evento IngresoProgramadoEliminadoEvent
/// y cancela el job recurrente en Hangfire.
/// </summary>
public sealed class IngresoProgramadoEliminadoEventHandler : INotificationHandler<IngresoProgramadoEliminadoEvent>
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<IngresoProgramadoEliminadoEventHandler> _logger;

    public IngresoProgramadoEliminadoEventHandler(
        IRecurringJobManager recurringJobManager,
        ILogger<IngresoProgramadoEliminadoEventHandler> logger)
    {
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public Task Handle(IngresoProgramadoEliminadoEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Usar el HangfireJobId (no el IngresoProgramadoId)
            var jobId = notification.HangfireJobId;

            // Eliminar el job recurrente de Hangfire
            _recurringJobManager.RemoveIfExists(jobId);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Job cancelado: {JobId} - Ingreso programado eliminado",
                    jobId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error al cancelar job de IngresoProgramado {IngresoProgramadoId}", 
                notification.IngresoProgramadoId);

            return Task.CompletedTask;
        }
    }
}
