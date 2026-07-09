using Kash.Shared.Application.Abstractions.Servicies;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Kash.Infrastructure.Services;

/// <summary>
/// Implementacin del servicio de cachdistribuida.
/// Utiliza IDistributedCache de .NET para abstraer el proveedor de cachsubyacente.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Obtiene un valor de la cachde forma asncrona.
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
    /// Establece un valor en la cachcon opciones de expiracin.
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
    /// Elimina un valor de la cach�.
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    /// <summary>
    /// Verifica si una clave existe en la cach�.
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// ?? LIMITACI�N: Invalida por patr�n (no soportado en IDistributedCache est�ndar).
    /// Esta implementaci�n no hace nada porque IDistributedCache no soporta pattern matching.
    /// 
    /// ?? Si necesitas esta funcionalidad:
    /// 1. Usa Redis con StackExchange.Redis directamente
    /// 2. O conf�a en el sistema de versionado de listas que invalida autom�ticamente
    /// 
    /// El sistema de versionado funciona as�:
    /// - Cada lista tiene una clave de versi�n: "list_version:Entity:UserId"
    /// - Cuando se hace CUD, se elimina esta clave
    /// - Al hacer GET, se genera una nueva versi�n
    /// - Las claves antiguas quedan hu�rfanas y expiran naturalmente
    /// </summary>
    public Task InvalidateByPatternAsync(string pattern)
    {
        // IDistributedCache est�ndar no soporta invalidaci�n por patr�n
        // El sistema de versionado de listas maneja esto autom�ticamente
        return Task.CompletedTask;
    }
}
