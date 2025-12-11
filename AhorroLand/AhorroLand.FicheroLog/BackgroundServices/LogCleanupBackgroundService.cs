using AhorroLand.FicheroLog.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AhorroLand.FicheroLog.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que ejecuta la limpieza periódica de logs
/// </summary>
public sealed class LogCleanupBackgroundService : BackgroundService
{
    private readonly LogCleanupService _cleanupService;
    private readonly ILogger<LogCleanupBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public LogCleanupBackgroundService(
        LogCleanupService cleanupService,
        ILogger<LogCleanupBackgroundService> logger)
    {
        _cleanupService = cleanupService;
        _logger = logger;
        _interval = TimeSpan.FromHours(24); // Ejecutar cada 24 horas
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Servicio de limpieza de logs iniciado. Se ejecutará cada {Interval} horas",
            _interval.TotalHours);

        // Esperar 1 minuto antes de la primera ejecución para que la app termine de iniciar
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Ejecutando limpieza periódica de logs...");
                await _cleanupService.CleanupOldLogsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza periódica de logs");
            }

            // Esperar hasta la próxima ejecución
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Servicio de limpieza de logs detenido");
    }
}
