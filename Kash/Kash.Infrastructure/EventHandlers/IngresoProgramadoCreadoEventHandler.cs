using Kash.Application.Features.IngresosProgramados.Commands.Execute;
using Kash.Domain.IngresosProgramados.Eventos;
using SergioIzq.Infrastructure.Kernel.Scheduling;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Infrastructure.EventHandlers;

/// <summary>
/// Handler de infraestructura que escucha el evento IngresoProgramadoCreadoEvent
/// y programa el job recurrente en Hangfire.
/// Optimizado para minimizar allocations y mejorar throughput.
/// </summary>
public sealed class IngresoProgramadoCreadoEventHandler : INotificationHandler<IngresoProgramadoCreadoEvent>
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<IngresoProgramadoCreadoEventHandler> _logger;

    public IngresoProgramadoCreadoEventHandler(
        IRecurringJobManager recurringJobManager,
        ILogger<IngresoProgramadoCreadoEventHandler> logger)
    {
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public Task Handle(IngresoProgramadoCreadoEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var cronExpression = CronExpressionConverter.ConvertirFrecuenciaACron(
                notification.Frecuencia.Value,
                notification.FechaEjecucion
            );

            var jobId = notification.IngresoProgramadoId.ToString();

            var jobOptions = new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            };

            _recurringJobManager.AddOrUpdate<IMediator>(
                jobId,
                mediator => mediator.Send(new ExecuteIngresoProgramadoCommand(notification.IngresoProgramadoId), CancellationToken.None),
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
            _logger.LogError(ex, "Error al programar IngresoProgramado {IngresoProgramadoId}", notification.IngresoProgramadoId);
            return Task.CompletedTask;
        }
    }
}
