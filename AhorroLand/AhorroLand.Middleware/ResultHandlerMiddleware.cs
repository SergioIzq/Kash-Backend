using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace AhorroLand.Middleware;

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

    // Opciones para lectura: Permite comentarios y comas al final para ser más tolerante
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

            // 1. Validaciones previas para no procesar innecesariamente
            if (context.Response.StatusCode == StatusCodes.Status200OK &&
                HasJsonContentType(context) &&
                !IsCompressed(context)) // IMPORTANTE: No leer si está comprimido
            {
                responseBody.Seek(0, SeekOrigin.Begin);

                // 2. OPTIMIZACIÓN: Obtener los bytes directamente sin convertir a String
                // Esto evita el error del BOM (0xEF) y ahorra memoria.
                if (responseBody.TryGetBuffer(out ArraySegment<byte> buffer) && buffer.Count > 0)
                {
                    if (TryDetectResultError(buffer, out var errorResponse, out var statusCode))
                    {
                        _logger.LogInformation(
                            "Result con error detectado: {ErrorCode} - Status: {StatusCode} - Path: {Path}",
                            errorResponse!.Code, statusCode, context.Request.Path);

                        await WriteErrorResponseAsync(context, originalBodyStream, errorResponse, statusCode);
                        return;
                    }
                }
            }

            // Si no hay error o no es procesable, devolver la respuesta original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico en ResultHandlerMiddleware. Path: {Path}", context.Request.Path);

            // Recuperación ante fallos: Devolver lo que haya en el buffer original
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
        // Si hay Content-Encoding (gzip, br, etc.), el body es binario, no texto.
        return context.Response.Headers.ContainsKey("Content-Encoding");
    }

    /// <summary>
    /// Detecta error parseando directamente los BYTES (ReadOnlyMemory)
    /// </summary>
    private bool TryDetectResultError(ReadOnlyMemory<byte> jsonBytes, out ErrorResponse? errorResponse, out int statusCode)
    {
        errorResponse = null;
        statusCode = StatusCodes.Status500InternalServerError;

        try
        {
            // JsonDocument.Parse maneja automáticamente el BOM UTF-8 (0xEF, 0xBB, 0xBF)
            // cuando trabaja con bytes.
            using var doc = JsonDocument.Parse(jsonBytes, DocumentOptions);
            var root = doc.RootElement;

            // Verificar si es un objeto JSON (si es array [], no es un Result)
            if (root.ValueKind != JsonValueKind.Object) return false;

            // Verificar propiedades (Case Insensitive implícito chequeando ambas)
            if (!root.TryGetProperty("isSuccess", out var isSuccessElement) &&
                !root.TryGetProperty("IsSuccess", out isSuccessElement))
            {
                return false;
            }

            if (isSuccessElement.GetBoolean())
            {
                return false; // Es Success = true, no hacemos nada
            }

            // Extraer el error
            if (!root.TryGetProperty("error", out var errorElement) &&
                !root.TryGetProperty("Error", out errorElement))
            {
                return false;
            }

            // Parsear propiedades del error con seguridad null
            var code = GetStringProperty(errorElement, "code") ?? "Error.Unknown";
            var name = GetStringProperty(errorElement, "name") ?? "Error";
            var message = GetStringProperty(errorElement, "message") ?? "Ocurrió un error";

            statusCode = MapErrorCodeToHttpStatus(code);
            errorResponse = new ErrorResponse(code, name, message, Activity.Current?.Id);

            return true;
        }
        catch (JsonException)
        {
            // Es común que falle si el body no es el JSON que esperamos. 
            // No logueamos error aquí para no ensuciar logs, simplemente retornamos false.
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error inesperado leyendo estructura Result.");
            return false;
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        // Validación de seguridad por si acaso
        if (string.IsNullOrEmpty(propertyName)) return null;

        // Intenta obtener la propiedad tal cual ("code")
        if (element.TryGetProperty(propertyName, out var prop))
        {
            return prop.GetString();
        }

        // Intenta obtener la propiedad en PascalCase ("Code")
        // ✅ CORRECTO: propertyName[1..] toma desde el 1 hasta el final automáticamente
        var pascalCaseName = char.ToUpper(propertyName[0]) + propertyName[1..];

        if (element.TryGetProperty(pascalCaseName, out prop))
        {
            return prop.GetString();
        }

        return null;
    }

    private static int MapErrorCodeToHttpStatus(string errorCode)
    {
        return errorCode switch
        {
            var c when c.Contains("Validation", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status400BadRequest,
            var c when c.Contains("NotFound", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status404NotFound,
            var c when c.Contains("Conflict", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
            var c when c.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
            var c when c.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status401Unauthorized,
            var c when c.Contains("Authentication", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status401Unauthorized,
            var c when c.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status403Forbidden,
            var c when c.Contains("Authorization", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status403Forbidden,
            var c when c.Contains("UpdateFailure", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status422UnprocessableEntity,
            var c when c.Contains("DeleteFailure", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Stream outputStream, ErrorResponse errorResponse, int statusCode)
    {
        // Importante: No podemos escribir cabeceras si ya empezaron a enviarse (aunque con MemoryStream intermedio es seguro)
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        // Removemos content-length anterior si existía porque el tamaño cambia
        context.Response.Headers.ContentLength = null;

        await JsonSerializer.SerializeAsync(outputStream, errorResponse, JsonOptions);
    }
}