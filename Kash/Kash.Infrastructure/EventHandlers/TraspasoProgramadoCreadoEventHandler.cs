using Kash.Application.Features.TraspasosProgramados.Commands.Execute;
using Kash.Domain.TraspasosProgramados.Eventos;
using SergioIzq.Infrastructure.Kernel.Scheduling;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Infrastructure.EventHandlers;

/// <summary>
/// Handler de infraestructura que escucha el evento TraspasoProgramadoCreadoEvent
/// y programa el job recurrente en Hangfire.
/// Optimizado para minimizar allocations y mejorar throughput.
/// </summary>
public sealed class TraspasoProgramadoCreadoEventHandler : INotificationHandler<TraspasoProgramadoCreadoEvent>
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<TraspasoProgramadoCreadoEventHandler> _logger;

    public TraspasoProgramadoCreadoEventHandler(
        IRecurringJobManager recurringJobManager,
        ILogger<TraspasoProgramadoCreadoEventHandler> logger)
    {
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public Task Handle(TraspasoProgramadoCreadoEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var cronExpression = CronExpressionConverter.ConvertirFrecuenciaACron(
                notification.Frecuencia.Value,
                notification.FechaEjecucion
            );

            var jobId = notification.TraspasoProgramadoId.ToString();

            var jobOptions = new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            };

            _recurringJobManager.AddOrUpdate<IMediator>(
                jobId,
                mediator => mediator.Send(new ExecuteTraspasoProgramadoCommand(notification.TraspasoProgramadoId), CancellationToken.None),
                cronExpression,
                jobOptions
            );

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Job programado: {JobId} con expresión CRON: {CronExpression}",
                    jobId,
                    cronExpression);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al programar TraspasoProgramado {TraspasoProgramadoId}", notification.TraspasoProgramadoId);
            return Task.CompletedTask;
        }
    }
}
