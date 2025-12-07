using AhorroLand.Shared.Application.Abstractions.Servicies;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AhorroLand.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de caché distribuida.
/// Utiliza IDistributedCache de .NET para abstraer el proveedor de caché subyacente.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Obtiene un valor de la caché de forma asíncrona.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedValue = await _cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(cachedValue))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Establece un valor en la caché con opciones de expiración.
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null)
    {
        if (value == null) return;

        var options = new DistributedCacheEntryOptions();

        if (slidingExpiration.HasValue)
        {
            options.SlidingExpiration = slidingExpiration.Value;
        }

        if (absoluteExpiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
        }

        // Valor por defecto: 5 minutos de sliding expiration (reducido de 1 hora)
        if (!slidingExpiration.HasValue && !absoluteExpiration.HasValue)
        {
            options.SlidingExpiration = TimeSpan.FromMinutes(5);
        }

        var jsonValue = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, jsonValue, options);
    }

    /// <summary>
    /// Elimina un valor de la caché.
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    /// <summary>
    /// Verifica si una clave existe en la caché.
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// ?? NUEVO: Invalida por patrón (no soportado en IDistributedCache estándar).
    /// Esta implementación no hace nada porque IDistributedCache no soporta pattern matching.
    /// Usa RedisCacheService si necesitas esta funcionalidad.
    /// </summary>
    public Task InvalidateByPatternAsync(string pattern)
    {
        // IDistributedCache estándar no soporta invalidación por patrón
        // Si usas MemoryCache, no necesitas esto de todas formas
        return Task.CompletedTask;
    }
}
