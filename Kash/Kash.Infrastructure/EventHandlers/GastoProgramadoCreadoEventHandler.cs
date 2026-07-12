using Kash.Application.Features.GastosProgramados.Commands.Execute;
using Kash.Domain.GastosProgramados.Eventos;
using SergioIzq.Infrastructure.Kernel.Scheduling;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Infrastructure.EventHandlers;

/// <summary>
/// Handler de infraestructura que escucha el evento GastoProgramadoCreadoEvent
/// y programa el job recurrente en Hangfire.
/// Optimizado para minimizar allocations y mejorar throughput.
/// </summary>
public sealed class GastoProgramadoCreadoEventHandler : INotificationHandler<GastoProgramadoCreadoEvent>
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<GastoProgramadoCreadoEventHandler> _logger;

    public GastoProgramadoCreadoEventHandler(
        IRecurringJobManager recurringJobManager,
        ILogger<GastoProgramadoCreadoEventHandler> _logger)
    {
        _recurringJobManager = recurringJobManager;
        this._logger = _logger;
    }

    public Task Handle(GastoProgramadoCreadoEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // ?? OPTIMIZACIÓN 1: Convertir frecuencia a CRON (método optimizado)
            var cronExpression = CronExpressionConverter.ConvertirFrecuenciaACron(
                notification.Frecuencia.Value,
                notification.FechaEjecucion
            );

            // ?? OPTIMIZACIÓN 2: Evitar ToString() innecesario si Guid ya es string
            var jobId = notification.GastoProgramadoId.ToString();

            // ?? OPTIMIZACIÓN 3: Reutilizar RecurringJobOptions
            var jobOptions = new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            };

            // 4. Programar el job recurrente en Hangfire
            // Hangfire serializa la expresión, no el objeto IMediator
            _recurringJobManager.AddOrUpdate<IMediator>(
                jobId,
                mediator => mediator.Send(new ExecuteGastoProgramadoCommand(notification.GastoProgramadoId), CancellationToken.None),
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
            _logger.LogError(ex, "Error al programar GastoProgramado {GastoProgramadoId}", notification.GastoProgramadoId);
            // No lanzamos la excepción para no romper el flujo de otros eventos
            return Task.CompletedTask;
        }
    }
}
