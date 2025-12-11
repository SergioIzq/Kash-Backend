using AhorroLand.FicheroLog.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AhorroLand.FicheroLog.Services;

/// <summary>
/// Servicio que limpia archivos de log antiguos
/// </summary>
public sealed class LogCleanupService
{
    private readonly HtmlFileLogOptions _options;
    private readonly ILogger<LogCleanupService> _logger;

    public LogCleanupService(IOptions<HtmlFileLogOptions> options, ILogger<LogCleanupService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Limpia archivos de log más antiguos que el número de días configurado
    /// </summary>
    public async Task CleanupOldLogsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(_options.LogDirectory))
            {
                _logger.LogInformation("El directorio de logs no existe: {LogDirectory}", _options.LogDirectory);
                return;
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-_options.CleanupAfterDays);
            var files = Directory.GetFiles(_options.LogDirectory, "*.html", SearchOption.TopDirectoryOnly);

            var deletedCount = 0;
            var totalSize = 0L;

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileInfo = new FileInfo(file);
                
                if (fileInfo.CreationTimeUtc < cutoffDate)
                {
                    totalSize += fileInfo.Length;
                    File.Delete(file);
                    deletedCount++;
                    
                    _logger.LogInformation(
                        "Archivo de log eliminado: {FileName}, Tamaño: {Size} bytes, Fecha: {Date}",
                        fileInfo.Name,
                        fileInfo.Length,
                        fileInfo.CreationTimeUtc);
                }
            }

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Limpieza completada: {DeletedCount} archivos eliminados, {TotalSize} bytes liberados",
                    deletedCount,
                    totalSize);
            }
            else
            {
                _logger.LogInformation("No hay archivos de log antiguos para eliminar");
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la limpieza de logs antiguos");
            throw;
        }
    }

    /// <summary>
    /// Limpia TODOS los archivos de log del directorio
    /// </summary>
    public async Task CleanupAllLogsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(_options.LogDirectory))
            {
                _logger.LogInformation("El directorio de logs no existe: {LogDirectory}", _options.LogDirectory);
                return;
            }

            var files = Directory.GetFiles(_options.LogDirectory, "*.html", SearchOption.TopDirectoryOnly);
            var deletedCount = 0;

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                File.Delete(file);
                deletedCount++;
            }

            _logger.LogInformation(
                "Limpieza total completada: {DeletedCount} archivos eliminados del directorio {LogDirectory}",
                deletedCount,
                _options.LogDirectory);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la limpieza total de logs");
            throw;
        }
    }
}
