using Kash.Domain.TraspasosProgramados.Eventos;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Infrastructure.EventHandlers;

/// <summary>
/// Handler de infraestructura que escucha el evento TraspasoProgramadoEliminadoEvent
/// y cancela el job recurrente en Hangfire.
/// </summary>
public sealed class TraspasoProgramadoEliminadoEventHandler : INotificationHandler<TraspasoProgramadoEliminadoEvent>
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<TraspasoProgramadoEliminadoEventHandler> _logger;

    public TraspasoProgramadoEliminadoEventHandler(
        IRecurringJobManager recurringJobManager,
        ILogger<TraspasoProgramadoEliminadoEventHandler> logger)
    {
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public Task Handle(TraspasoProgramadoEliminadoEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var jobId = notification.HangfireJobId;

            // Eliminar el job recurrente de Hangfire
            _recurringJobManager.RemoveIfExists(jobId);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Job cancelado: {JobId} - Traspaso programado eliminado",
                    jobId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error al cancelar job de TraspasoProgramado {TraspasoProgramadoId}", 
                notification.TraspasoProgramadoId);
            
            return Task.CompletedTask;
        }
    }
}
