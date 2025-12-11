namespace AhorroLand.FicheroLog.Configuration;

public sealed class HtmlFileLogOptions
{
    /// <summary>
    /// Ruta donde se guardarán los archivos de log HTML
    /// </summary>
    public string LogDirectory { get; set; } = "/app/logs";

    /// <summary>
    /// Prefijo del nombre de archivo (ej: "ahorroland-")
    /// </summary>
    public string FileNamePrefix { get; set; } = "ahorroland-";

    /// <summary>
    /// Intervalo de rotación de archivos (Day, Hour, etc.)
    /// </summary>
    public Serilog.RollingInterval RollingInterval { get; set; } = Serilog.RollingInterval.Day;

    /// <summary>
    /// Número máximo de archivos de log a retener (null = sin límite)
    /// </summary>
    public int? RetainedFileCountLimit { get; set; } = 7;

    /// <summary>
    /// Tamaño máximo de cada archivo en bytes (10 MB por defecto)
    /// </summary>
    public long? FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Si debe crear un nuevo archivo cuando se alcance el límite de tamaño
    /// </summary>
    public bool RollOnFileSizeLimit { get; set; } = true;

    /// <summary>
    /// Días de antigüedad después de los cuales se limpian todos los logs
    /// </summary>
    public int CleanupAfterDays { get; set; } = 30;

    /// <summary>
    /// Si debe ejecutar limpieza al iniciar
    /// </summary>
    public bool CleanOnStartup { get; set; } = true;

    /// <summary>
    /// Título del documento HTML
    /// </summary>
    public string PageTitle { get; set; } = "AhorroLand - Logs del Sistema";

    /// <summary>
    /// Si debe incluir logs de nivel Information relacionados con BD
    /// </summary>
    public bool IncludeDatabaseOperations { get; set; } = true;

    /// <summary>
    /// Si debe incluir logs de nivel Warning
    /// </summary>
    public bool IncludeWarnings { get; set; } = true;

    /// <summary>
    /// Si debe incluir logs de nivel Error
    /// </summary>
    public bool IncludeErrors { get; set; } = true;
}
