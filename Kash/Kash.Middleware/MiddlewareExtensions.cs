using Microsoft.AspNetCore.Builder;

namespace Kash.Middleware;

/// <summary>
/// Extensiones para registrar los middlewares de manejo de excepciones y resultados
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Registra el middleware de manejo global de excepciones
    /// DEBE ser uno de los primeros middlewares en el pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandler>();
    }

    /// <summary>
    /// Registra el middleware de manejo de objetos Result con errores
    /// DEBE registrarse DESPUÉS de UseRouting() pero ANTES de UseEndpoints()
    /// </summary>
    public static IApplicationBuilder UseResultHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ResultHandlerMiddleware>();
    }

    /// <summary>
    /// ?? NUEVO: Registra el middleware que deshabilita el caché HTTP del navegador.
    /// Previene que Ctrl+R use datos cacheados obsoletos.
    /// DEBE registrarse ANTES de UseResponseCompression() para que los headers se apliquen correctamente.
    /// </summary>
    public static IApplicationBuilder UseNoCache(this IApplicationBuilder app)
    {
        return app.UseMiddleware<NoCacheMiddleware>();
    }

    /// <summary>
    /// Registra todos los middlewares de Kash en el orden correcto
    /// </summary>
    public static IApplicationBuilder UseKashExceptionHandling(this IApplicationBuilder app)
    {
        app.UseGlobalExceptionHandler();
        app.UseResultHandler();
        app.UseNoCache(); // ?? Agregamos el middleware de no-caché
        return app;
    }
}
