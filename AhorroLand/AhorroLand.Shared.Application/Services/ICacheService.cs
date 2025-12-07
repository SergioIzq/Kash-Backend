namespace AhorroLand.Shared.Application.Abstractions.Servicies;

/// <summary>
/// Proporciona métodos asíncronos y con expiración controlada para la gestión de caché.
/// Prioriza la velocidad (async) y el bajo consumo de memoria (control de tiempo).
/// </summary>
public interface ICacheService
{
    // --- 1. Lectura de Cache (Velocidad) ---

    /// <summary>
    /// Obtiene de forma asíncrona un valor de la caché.
    /// </summary>
    /// <typeparam name="T">El tipo del valor a recuperar.</typeparam>
    /// <param name="key">La clave única del elemento.</param>
    /// <returns>El valor cacheado o el valor por defecto de T si no se encuentra.</returns>
    Task<T?> GetAsync<T>(string key);

    // --- 2. Escritura de Cache (Control de Memoria) ---

    /// <summary>
    /// Establece un valor en la caché con un tiempo de expiración sliding y absolute.
    /// </summary>
    /// <typeparam name="T">El tipo del valor a guardar.</typeparam>
    /// <param name="key">La clave única del elemento.</param>
    /// <param name="value">El valor a cachear.</param>
    /// <param name="slidingExpiration">Tiempo de inactividad antes de expirar. Debe usarse para liberar memoria.</param>
    /// <param name="absoluteExpiration">Tiempo máximo de vida absoluto, independientemente de la actividad.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null);

    // --- 3. Validación y Liberación de Cache (Memoria y Consistencia) ---

    /// <summary>
    /// Elimina un elemento de la caché de forma explícita.
    /// Es crucial para mantener la consistencia y liberar memoria de objetos obsoletos.
    /// </summary>
    /// <param name="key">La clave del elemento a eliminar.</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Comprueba si existe un elemento en la caché.
    /// </summary>
    /// <param name="key">La clave del elemento.</param>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// 🔥 NUEVO: Invalida todas las claves que coincidan con un patrón.
    /// Útil para invalidar todas las paginaciones/búsquedas de una entidad.
    /// </summary>
    /// <param name="pattern">Patrón de búsqueda (ej: "Gasto:*")</param>
    Task InvalidateByPatternAsync(string pattern);
}