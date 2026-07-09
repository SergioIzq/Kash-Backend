using Kash.Shared.Domain.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kash.Middleware;

public sealed class ResultHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResultHandlerMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    public ResultHandlerMiddleware(RequestDelegate next, ILogger<ResultHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // 1. Interceptamos SOLO si es JSON y no está comprimido
            if (HasJsonContentType(context) && !IsCompressed(context))
            {
                responseBody.Seek(0, SeekOrigin.Begin);

                // Leemos el buffer sin convertir a string para alto rendimiento
                if (responseBody.TryGetBuffer(out ArraySegment<byte> buffer) && buffer.Count > 0)
                {
                    // 2. Analizamos si el cuerpo es un Result Fallido
                    if (TryDetectFailureInBody(buffer, out var error, out var suggestedStatusCode))
                    {
                        // 3. LOGGING: Registramos el error de negocio detectado
                        _logger.LogInformation(
                            "Result Fallido detectado en middleware: {Code} - {Name} | Sugerido: {Status} | Actual: {CurrentStatus}",
                            error!.Code, error.Name, suggestedStatusCode, context.Response.StatusCode);

                        // 4. CORRECCIÓN AUTOMÁTICA (Soft 200 check)
                        // Si el controlador devolvió 200 OK pero el contenido es un Error, lo corregimos.
                        if (context.Response.StatusCode == StatusCodes.Status200OK && suggestedStatusCode != StatusCodes.Status200OK)
                        {
                            _logger.LogWarning("Corrigiendo respuesta HTTP 200 OK a {StatusCode} porque el Result contiene errores.", suggestedStatusCode);

                            // Reescribimos la respuesta asegurando el formato Result correcto
                            await WriteErrorResponseAsync(context, originalBodyStream, error, suggestedStatusCode);
                            return;
                        }
                    }
                }
            }

            // Si no hubo nada que corregir, devolvemos la respuesta original tal cual
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico en ResultHandlerMiddleware procesando la respuesta.");

            // Si ocurre un error aquí, devolvemos lo que teníamos en el buffer original para no romper la respuesta
            // (El GlobalExceptionHandler que está más arriba en la pila capturará la excepción si la relanzamos)
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static bool HasJsonContentType(HttpContext context)
    {
        var contentType = context.Response.ContentType;
        return !string.IsNullOrEmpty(contentType) && contentType.Contains("application/json");
    }

    private static bool IsCompressed(HttpContext context)
    {
        return context.Response.Headers.ContainsKey("Content-Encoding");
    }

    /// <summary>
    /// Analiza los bytes del body para ver si es un JSON con estructura { "isSuccess": false, "error": { ... } }
    /// </summary>
    private bool TryDetectFailureInBody(ReadOnlyMemory<byte> jsonBytes, out Error? error, out int suggestedStatusCode)
    {
        error = null;
        suggestedStatusCode = StatusCodes.Status200OK;

        try
        {
            using var doc = JsonDocument.Parse(jsonBytes, DocumentOptions);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object) return false;

            // 1. Verificar "isSuccess"
            // Buscamos camelCase y PascalCase por compatibilidad
            if (!TryGetBooleanProperty(root, "isSuccess", out var isSuccess))
            {
                // Si no tiene propiedad isSuccess, no es un Result, ignoramos.
                return false;
            }

            if (isSuccess) return false; // Es un éxito, no hay nada que hacer.

            // 2. Es un fallo: Extraer el objeto "error"
            if (!TryGetProperty(root, "error", out var errorElement))
            {
                return false; // Dice false pero no tiene error, estructura inválida.
            }

            // 3. Extraer detalles del error
            var code = GetStringProperty(errorElement, "code") ?? "Error.Unknown";
            var name = GetStringProperty(errorElement, "name") ?? "Error";
            var message = GetStringProperty(errorElement, "message") ?? "Ocurrió un error no especificado";

            // Reconstruimos el objeto Error de dominio
            error = new Error(code, name, message);

            // Calculamos qué status code debería tener
            suggestedStatusCode = MapErrorCodeToHttpStatus(code);

            return true;
        }
        catch (Exception)
        {
            // Si falla el parseo, asumimos que no es un Result nuestro y dejamos pasar.
            return false;
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Stream outputStream, Error error, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        context.Response.Headers.ContentLength = null; // El tamaño va a cambiar

        // CLAVE: Usamos Result.Failure para mantener la consistencia con el GlobalExceptionHandler
        var result = Result.Failure(error);

        await JsonSerializer.SerializeAsync(outputStream, result, JsonOptions);
    }

    // --- Helpers de Parseo JSON Robustos ---

    private static bool TryGetBooleanProperty(JsonElement element, string propertyName, out bool value)
    {
        value = false;
        if (element.TryGetProperty(propertyName, out var prop) ||
            element.TryGetProperty(ToPascalCase(propertyName), out prop))
        {
            if (prop.ValueKind == JsonValueKind.True || prop.ValueKind == JsonValueKind.False)
            {
                value = prop.GetBoolean();
                return true;
            }
        }
        return false;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value)) return true;
        if (element.TryGetProperty(ToPascalCase(propertyName), out value)) return true;

        value = default;
        return false;
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        if (TryGetProperty(element, propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString();
        }
        return null;
    }

    private static string ToPascalCase(string str) =>
        string.IsNullOrEmpty(str) ? str : char.ToUpper(str[0]) + str[1..];

    /// <summary>
    /// Mapeo heurístico basado en strings por si no tenemos el Enum disponible en el JSON crudo.
    /// Intenta coincidir con la lógica del AbsController.
    /// </summary>
    private static int MapErrorCodeToHttpStatus(string errorCode)
    {
        return errorCode switch
        {
            // 400 Bad Request (Validación)
            var c when Contains(c, "Validation") || Contains(c, "NullValue") || Contains(c, "InvalidFormat") => StatusCodes.Status400BadRequest,

            // 404 Not Found
            var c when Contains(c, "NotFound") => StatusCodes.Status404NotFound,

            // 409 Conflict (Duplicados, Reglas de negocio)
            var c when Contains(c, "Conflict") || Contains(c, "Duplicate") || Contains(c, "AlreadyExists") => StatusCodes.Status409Conflict,

            // 401 Unauthorized (Login fallido, Token inválido)
            var c when Contains(c, "Unauthorized") || Contains(c, "InvalidCredentials") || Contains(c, "TokenExpired") => StatusCodes.Status401Unauthorized,

            // 403 Forbidden (Permisos)
            var c when Contains(c, "Forbidden") || Contains(c, "AccessDenied") => StatusCodes.Status403Forbidden,

            // 503 Service Unavailable
            var c when Contains(c, "Unavailable") || Contains(c, "ConnectionError") => StatusCodes.Status503ServiceUnavailable,

            // Default 500
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static bool Contains(string source, string toCheck) =>
        source.Contains(toCheck, StringComparison.OrdinalIgnoreCase);
}
