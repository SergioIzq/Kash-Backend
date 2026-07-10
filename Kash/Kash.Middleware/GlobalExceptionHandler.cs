using SergioIzq.Domain.Kernel.Abstractions.Errors;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Diagnostics;
using System.Text.Json;

namespace Kash.Middleware;

public sealed class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    // Configuración JSON para que coincida con tus respuestas de API (camelCase)
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
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
        var stopwatch = Stopwatch.StartNew();

        // 1. Obtenemos el Error de Dominio y el Status Code HTTP
        var (statusCode, domainError) = MapExceptionToError(exception);

        // 2. Logging
        LogException(exception, context, statusCode, domainError);

        // 3. Preparamos la respuesta HTTP
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        // 4. IMPORTANTE: Envolvemos el error en tu estructura Result estandarizada
        var result = Result.Failure(domainError);

        // 5. Serializamos directamente al Body
        await JsonSerializer.SerializeAsync(context.Response.Body, result, JsonOptions);

        stopwatch.Stop();
        // Solo logueamos performance si fue lento (>500ms) para no ensuciar logs
        if (stopwatch.ElapsedMilliseconds > 500)
        {
            _logger.LogDebug("Respuesta de excepción enviada en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Transforma Excepciones de .NET/MySQL en tu record 'Error' del Dominio
    /// </summary>
    private (int statusCode, Error error) MapExceptionToError(Exception exception)
    {
        return exception switch
        {
            // --- MySQL Specifics ---
            MySqlException mySqlEx => HandleMySqlException(mySqlEx),

            // --- .NET Validations ---
            ArgumentNullException argNull => (StatusCodes.Status400BadRequest,
                Error.Validation($"El parámetro '{argNull.ParamName}' es requerido.")),

            ArgumentException arg => (StatusCodes.Status400BadRequest,
                Error.Validation(arg.Message)),

            // --- Clean Architecture Standard Exceptions ---
            KeyNotFoundException notFound => (StatusCodes.Status404NotFound,
                Error.NotFound(notFound.Message)),

            UnauthorizedAccessException _ => (StatusCodes.Status403Forbidden,
                new Error("Auth.Forbidden", "Acceso Denegado", "No tienes permisos para ejecutar esta acción.")),

            TimeoutException _ => (StatusCodes.Status408RequestTimeout,
                new Error("Server.Timeout", "Tiempo de espera agotado", "La operación tardó demasiado.")),

            NotSupportedException notSupported => (StatusCodes.Status501NotImplemented,
                new Error("Server.NotSupported", "No soportado", notSupported.Message)),

            // --- Catch All (Error 500) ---
            // Usamos un error genérico de sistema definido abajo o en tus constantes
            _ => (StatusCodes.Status500InternalServerError,
                SystemErrors.InternalServerError)
        };
    }

    private (int statusCode, Error error) HandleMySqlException(MySqlException mySqlEx)
    {
        // Mapeamos los códigos de error de MySQL a tu estructura Error
        return mySqlEx.ErrorCode switch
        {
            MySqlErrorCode.DuplicateKeyEntry => (StatusCodes.Status409Conflict,
                new Error("Data.Duplicate", "Registro Duplicado", "Ya existe un registro con estos datos únicos.")),

            MySqlErrorCode.RowIsReferenced or MySqlErrorCode.RowIsReferenced2 => (StatusCodes.Status409Conflict,
                new Error("Delete.Error", "Error en eliminar", "El registro no se puede eliminar porque está siendo usado por otros datos.")),

            MySqlErrorCode.UnableToConnectToHost or MySqlErrorCode.ConnectionCountError => (StatusCodes.Status503ServiceUnavailable,
                new Error("Data.Unavailable", "Servicio no disponible", "No se pudo conectar con la base de datos.")),

            // Fallback para otros errores de BD
            _ => (StatusCodes.Status500InternalServerError,
                new Error("Data.SqlError", "Error de Base de Datos", "Ocurrió un error técnico al procesar los datos."))
        };
    }

    private void LogException(Exception exception, HttpContext context, int statusCode, Error error)
    {
        var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;

        // Mensaje de log estructurado para herramientas como Seq, ELK o AppInsights
        _logger.Log(logLevel, exception,
            "[{ErrorCode}] {ErrorName}: {ErrorMessage} | Status: {StatusCode} | Path: {Path}",
            error.Code, error.Name, error.Message, statusCode, context.Request.Path);
    }
}
