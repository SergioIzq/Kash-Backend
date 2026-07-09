using Kash.FicheroLog.BackgroundServices;
using Kash.FicheroLog.Configuration;
using Kash.FicheroLog.Filters;
using Kash.FicheroLog.Formatters;
using Kash.FicheroLog.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;

namespace Kash.FicheroLog;

/// <summary>
/// Extensiones para configurar el sistema de logging en HTML
/// </summary>
public static class HtmlFileLogExtensions
{
    /// <summary>
    /// Agrega el sistema de logging HTML a la aplicación
    /// </summary>
    public static IServiceCollection AddHtmlFileLogging(
        this IServiceCollection services,
        Action<HtmlFileLogOptions>? configureOptions = null)
    {
        // Configurar opciones
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<HtmlFileLogOptions>(options => { });
        }

        // Registrar servicios
        services.AddSingleton<LogCleanupService>();
        services.AddHostedService<LogCleanupBackgroundService>();

        return services;
    }

    /// <summary>
    /// Configura Serilog para escribir logs en archivos HTML con filtrado automático
    /// FIX: Ahora usa HtmlLogHooks para estructura HTML completa
    /// </summary>
    public static LoggerConfiguration WriteToHtmlFile(
        this LoggerSinkConfiguration sinkConfiguration,
        HtmlFileLogOptions? options = null)
    {
        options ??= new HtmlFileLogOptions();

        // Asegurar que el directorio existe
        if (!Directory.Exists(options.LogDirectory))
        {
            Directory.CreateDirectory(options.LogDirectory);
        }

        // Determinar el patrón de archivo basado en el intervalo
        var filePattern = GetFilePattern(options);
        var filePath = Path.Combine(options.LogDirectory, filePattern);

        // FIX: Agregar hooks para estructura HTML completa
        return sinkConfiguration.File(
            formatter: new HtmlLogFormatter(),
            path: filePath,
            rollingInterval: options.RollingInterval,
            retainedFileCountLimit: options.RetainedFileCountLimit,
            fileSizeLimitBytes: options.FileSizeLimitBytes,
            rollOnFileSizeLimit: options.RollOnFileSizeLimit,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1),
            hooks: new HtmlLogHooks(options.PageTitle) // IMPORTANTE: Agregar hooks
        ).Filter.ByIncludingOnly(logEvent =>
            DatabaseAndErrorsFilter.ShouldInclude(
                logEvent,
                options.IncludeDatabaseOperations,
                options.IncludeWarnings,
                options.IncludeErrors));
    }

    private static string GetFilePattern(HtmlFileLogOptions options)
    {
        var baseName = options.FileNamePrefix;

        // Asegurar que termine en guion si no está vacío
        if (!baseName.EndsWith("-") && !string.IsNullOrEmpty(baseName))
        {
            baseName += "-";
        }

        return $"{baseName}.html";
    }
}
