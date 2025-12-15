using Microsoft.AspNetCore.Http;

namespace Kash.Middleware;

public sealed class NoCacheMiddleware
{
    private readonly RequestDelegate _next;

    public NoCacheMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Hook: Esto se ejecutará justo antes de enviar los headers a la red,
        // pero DESPUÉS de que el controlador haya definido el Content-Type.
        context.Response.OnStarting(() =>
        {
            if (IsApiResponse(context))
            {
                var headers = context.Response.Headers;

                // Sobrescribimos cualquier valor previo para asegurar el no-cache
                headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
                headers["Pragma"] = "no-cache";
                headers["Expires"] = "0";
            }
            return Task.CompletedTask;
        });

        // Continuar con el pipeline
        await _next(context);
    }

    private static bool IsApiResponse(HttpContext context)
    {
        var contentType = context.Response.ContentType;

        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}