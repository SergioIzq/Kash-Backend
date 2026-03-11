using Kash.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kash.Infrastructure.Services;

public sealed class IsinResolverService : IIsinResolverService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IsinResolverService> _logger;
    private static readonly ConcurrentDictionary<string, string> _cache = new();

    public IsinResolverService(IHttpClientFactory httpClientFactory, ILogger<IsinResolverService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> ResolveAsync(string isin, CancellationToken cancellationToken = default)
    {
        // Si ya lo buscamos antes, lo devolvemos inmediatamente sin llamar a la API
        if (_cache.TryGetValue(isin, out var cachedSymbol))
        {
            return cachedSymbol;
        }

        int maxRetries = 3;

        for (int intento = 1; intento <= maxRetries; intento++)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("YahooFinance");

                // 1. EL TRUCO: Disfrazamos la petición como si fuera un navegador Chrome real
                using var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/finance/search?q={isin}");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9");

                // Enviamos la petición modificada
                var response = await client.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Yahoo Finance devolvió 429. Intento {Intento} de {Max}. Esperando...", intento, maxRetries);

                    if (intento == maxRetries) break;

                    // Aumentamos un poco el tiempo base de espera por si acaso (2 segundos x intento)
                    await Task.Delay(2000 * intento, cancellationToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

                // ... (El resto del código de búsqueda de ".DE", "EUR", etc. se mantiene igual)
                if (jsonResponse.TryGetProperty("quotes", out var quotesElement) &&
                    quotesElement.ValueKind == System.Text.Json.JsonValueKind.Array &&
                    quotesElement.GetArrayLength() > 0)
                {
                    var quotes = quotesElement.EnumerateArray().ToList();

                    var bestMatch = quotes.FirstOrDefault(q =>
                        q.TryGetProperty("symbol", out var sym) &&
                        (sym.GetString()!.EndsWith(".DE") || sym.GetString()!.EndsWith(".F")));

                    if (bestMatch.ValueKind == System.Text.Json.JsonValueKind.Undefined)
                        bestMatch = quotes.FirstOrDefault(q => q.TryGetProperty("currency", out var curr) && curr.GetString() == "EUR");

                    if (bestMatch.ValueKind == System.Text.Json.JsonValueKind.Undefined)
                        bestMatch = quotes.First();

                    if (bestMatch.TryGetProperty("symbol", out var symbolElement))
                    {
                        var symbol = symbolElement.GetString();
                        _cache.TryAdd(isin, symbol ?? isin);
                        return symbol ?? isin;
                    }
                }

                _logger.LogWarning("Yahoo Finance no encontró el ISIN {Isin}", isin);
                _cache.TryAdd(isin, isin);
                return isin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando a Yahoo Finance en el intento {Intento} para el ISIN {Isin}", intento, isin);

                if (intento == maxRetries) break;
                await Task.Delay(2000 * intento, cancellationToken);
            }
        }

        // Si después de todos los reintentos falla, devolvemos el ISIN original
        _logger.LogWarning("Se agotaron los reintentos para {Isin}. Se usará el ISIN como ticker.", isin);
        return isin;
    }
}
