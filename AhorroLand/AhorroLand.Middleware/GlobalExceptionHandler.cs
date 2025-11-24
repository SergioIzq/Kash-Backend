using AhorroLand.Shared.Domain.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using MySqlConnector; // 🔥 NUEVO: Para capturar errores de MySQL

namespace AhorroLand.Middleware;

/// <summary>
/// Middleware optimizado para manejo global de excepciones y objetos Result con errores.
/// Enfocado en rendimiento y mensajes claros para el usuario.
/// </summary>
public sealed class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    // ✅ OPTIMIZACIÓN: JsonSerializerOptions reutilizable (evita crear nuevas instancias en cada request)
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false // Más rápido sin indentación
    };

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // ✅ OPTIMIZACIÓN: Usar Stopwatch para medir el tiempo de respuesta de error
        var stopwatch = Stopwatch.StartNew();

        var (statusCode, errorResponse) = MapExceptionToResponse(exception);

        // Logging estructurado
        LogException(exception, context, statusCode);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        // ✅ OPTIMIZACIÓN: Serialización directa al stream (sin buffer intermedio)
        await JsonSerializer.SerializeAsync(context.Response.Body, errorResponse, JsonOptions);

        stopwatch.Stop();
        _logger.LogDebug("Respuesta de error enviada en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Mapea excepciones a respuestas HTTP con códigos de estado apropiados
    /// </summary>
    private (int statusCode, ErrorResponse response) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            // 🔥 NUEVO: Errores de base de datos MySQL
            MySqlException mySqlEx => HandleMySqlException(mySqlEx),

            // Excepciones de validación
            ArgumentNullException argNull =>
                (StatusCodes.Status400BadRequest,
              new ErrorResponse("Validation.ArgumentNull",
          "Argumento requerido",
           $"El parámetro '{argNull.ParamName}' no puede ser nulo.")),

            ArgumentException arg =>
       (StatusCodes.Status400BadRequest,
      new ErrorResponse("Validation.ArgumentInvalid",
    "Argumento inválido",
     arg.Message)),

            // Operaciones inválidas
            InvalidOperationException invalid =>
   (StatusCodes.Status400BadRequest,
             new ErrorResponse("Operation.Invalid",
    "Operación inválida",
       invalid.Message)),

            // Recursos no encontrados
            KeyNotFoundException notFound =>
            (StatusCodes.Status404NotFound,
           new ErrorResponse("Resource.NotFound",
 "Recurso no encontrado",
           notFound.Message)),

            // Acceso no autorizado
            UnauthorizedAccessException _ =>
     (StatusCodes.Status403Forbidden,
       new ErrorResponse("Access.Forbidden",
 "Acceso denegado",
      "No tienes permisos para realizar esta operación.")),

            // Timeout
            TimeoutException _ =>
                 (StatusCodes.Status408RequestTimeout,
            new ErrorResponse("Request.Timeout",
             "Tiempo de espera agotado",
               "La operación tardó demasiado tiempo. Por favor, intenta nuevamente.")),

            // Operaciones no soportadas
            NotSupportedException notSupported =>
    (StatusCodes.Status501NotImplemented,
   new ErrorResponse("Operation.NotSupported",
    "Operación no soportada",
    notSupported.Message)),

            // Error genérico (500)
            _ => (StatusCodes.Status500InternalServerError,
                 new ErrorResponse("Server.InternalError",
                  "Error interno del servidor",
                       "Ocurrió un error inesperado. Por favor, contacta con soporte."))
        };
    }

    /// <summary>
    /// 🔥 NUEVO: Maneja errores específicos de MySQL con mensajes amigables
    /// </summary>
    private (int statusCode, ErrorResponse response) HandleMySqlException(MySqlException mySqlEx)
    {
        return mySqlEx.ErrorCode switch
        {
            // Columna ambigua (Column 'X' in where clause is ambiguous)
            MySqlErrorCode.NonUniqueTable or MySqlErrorCode.BadFieldError =>
       (StatusCodes.Status500InternalServerError,
            new ErrorResponse("Database.AmbiguousColumn",
          "Error en la consulta de base de datos",
                 "Se detectó un problema en la estructura de la consulta. Por favor, contacta con soporte técnico.",
   Activity.Current?.Id)),

            // Violación de clave única (Duplicate entry)
            MySqlErrorCode.DuplicateKeyEntry =>
     (StatusCodes.Status409Conflict,
                 new ErrorResponse("Database.DuplicateEntry",
            "Registro duplicado",
      "Ya existe un registro con los mismos datos únicos.")),

            // Violación de clave foránea (Cannot delete or update a parent row)
            MySqlErrorCode.RowIsReferenced or MySqlErrorCode.RowIsReferenced2 =>
      (StatusCodes.Status409Conflict,
       new ErrorResponse("Database.ForeignKeyViolation",
             "No se puede eliminar el registro",
 "El registro está siendo usado por otros datos y no puede ser eliminado.")),

            // Conexión perdida
            MySqlErrorCode.UnableToConnectToHost or MySqlErrorCode.ConnectionCountError =>
                  (StatusCodes.Status503ServiceUnavailable,
                            new ErrorResponse("Database.ConnectionError",
                  "Error de conexión con la base de datos",
                   "No se pudo conectar con la base de datos. Por favor, intenta más tarde.")),

            // Tabla no existe
            MySqlErrorCode.NoSuchTable =>
     (StatusCodes.Status500InternalServerError,
   new ErrorResponse("Database.TableNotFound",
                "Error de estructura de base de datos",
         "Se intentó acceder a una tabla que no existe. Contacta con soporte técnico.")),

            // Error genérico de MySQL
            _ => (StatusCodes.Status500InternalServerError,
              new ErrorResponse("Database.Error",
           "Error de base de datos",
      $"Ocurrió un error en la base de datos: {mySqlEx.Message}",
           Activity.Current?.Id))
        };
    }

    /// <summary>
    /// Logging estructurado optimizado
    /// </summary>
    private void LogException(Exception exception, HttpContext context, int statusCode)
    {
        var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;

        // 🔥 NUEVO: Log adicional para errores de MySQL con detalles técnicos
        if (exception is MySqlException mySqlEx)
        {
            _logger.Log(logLevel, mySqlEx,
            "Error MySQL: {ErrorCode} - {SqlState} - Status: {StatusCode} - Path: {Path} - Method: {Method} - TraceId: {TraceId}",
               mySqlEx.ErrorCode,
          mySqlEx.SqlState,
                 statusCode,
                   context.Request.Path,
           context.Request.Method,
           Activity.Current?.Id ?? context.TraceIdentifier);
        }
        else
        {
            _logger.Log(logLevel, exception,
         "Error manejado: {ExceptionType} - Status: {StatusCode} - Path: {Path} - Method: {Method} - TraceId: {TraceId}",
          exception.GetType().Name,
                   statusCode,
                   context.Request.Path,
          context.Request.Method,
                       Activity.Current?.Id ?? context.TraceIdentifier);
        }
    }
}

/// <summary>
/// Respuesta de error estandarizada
/// </summary>
public sealed record ErrorResponse
{
    public string Code { get; init; }
    public string Title { get; init; }
    public string Detail { get; init; }
    public string? TraceId { get; init; }
    public DateTime Timestamp { get; init; }

    public ErrorResponse(string code, string title, string detail, string? traceId = null)
    {
        Code = code;
        Title = title;
        Detail = detail;
        TraceId = traceId ?? Activity.Current?.Id;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Crea una respuesta de error desde un objeto Error del dominio
    /// </summary>
    public static ErrorResponse FromDomainError(Error error, string? traceId = null)
    {
        return new ErrorResponse(
         error.Code,
        error.Name,
      error.Message ?? "No se proporcionó detalle adicional.",
    traceId
  );
    }
}
