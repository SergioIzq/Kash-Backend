using Microsoft.AspNetCore.Http;

namespace Kash.Middleware;

/// <summary>
/// Middleware que deshabilita completamente el caché HTTP del navegador.
/// Agrega headers que fuerzan al navegador a revalidar siempre con el servidor.
/// Soluciona el problema de que Ctrl+R use caché mientras que Ctrl+Shift+R lo ignore.
/// </summary>
public sealed class NoCacheMiddleware
{
    private readonly RequestDelegate _next;

    public NoCacheMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ejecutar el pipeline primero para generar la respuesta
        await _next(context);

        // Solo aplicar a respuestas de API (JSON)
        // No aplicar a archivos estáticos (CSS, JS, imágenes)
        if (IsApiResponse(context))
        {
            // ?? HEADERS CRÍTICOS: Deshabilitan caché HTTP del navegador
            context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
        }
    }

    private static bool IsApiResponse(HttpContext context)
    {
        // Verificar si la respuesta es JSON (API)
        var contentType = context.Response.ContentType;
        
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
