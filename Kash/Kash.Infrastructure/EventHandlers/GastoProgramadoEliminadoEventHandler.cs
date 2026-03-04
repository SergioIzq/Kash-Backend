using Kash.Domain.GastosProgramados.Eventos;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Infrastructure.EventHandlers;

/// <summary>
/// Handler de infraestructura que escucha el evento GastoProgramadoEliminadoEvent
/// y cancela el job recurrente en Hangfire.
/// </summary>
public sealed class GastoProgramadoEliminadoEventHandler : INotificationHandler<GastoProgramadoEliminadoEvent>
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<GastoProgramadoEliminadoEventHandler> _logger;

    public GastoProgramadoEliminadoEventHandler(
        IRecurringJobManager recurringJobManager,
        ILogger<GastoProgramadoEliminadoEventHandler> logger)
    {
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public Task Handle(GastoProgramadoEliminadoEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var jobId = notification.HangfireJobId;

            // Eliminar el job recurrente de Hangfire
            _recurringJobManager.RemoveIfExists(jobId);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Job cancelado: {JobId} - Gasto programado eliminado",
                    jobId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error al cancelar job de GastoProgramado {GastoProgramadoId}", 
                notification.GastoProgramadoId);
            
            return Task.CompletedTask;
        }
    }
}
