using Kash.Application.Interfaces;
using Kash.Shared.Application.Dtos;
using MediatR;
using SergioIzq.Application.Kernel.Services;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Gastos.Queries.Habituales;

/// <summary>
/// No hereda de ningún handler base del kernel: agrupa y cuenta combinaciones (agregado),
/// no pagina ni lista entidades individuales.
///
/// Caché de 30s con clave única POR USUARIO (sin sufijo de `limit`): se cachea siempre el
/// top-<see cref="MaxCacheable"/> y se recorta en memoria al `limit` pedido, precisamente para
/// que exista una única clave invalidable con `RemoveAsync` exacto tras crear un gasto (ver
/// <see cref="CacheKey"/>, usado también por `CreateGastoCommandHandler.OnEntityCreatedAsync`).
/// Si se pide un `limit` mayor que <see cref="MaxCacheable"/> se consulta sin caché.
/// </summary>
public sealed class GetHabitualesGastosQueryHandler
    : IRequestHandler<GetHabitualesGastosQuery, Result<IReadOnlyList<GastoHabitualDto>>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
    private const int MaxCacheable = 20;

    private readonly IGastoHabitualesRepository _repository;
    private readonly ICacheService _cacheService;

    public GetHabitualesGastosQueryHandler(IGastoHabitualesRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public static string CacheKey(Guid usuarioId) => $"gasto_habituales:{usuarioId}";

    public async Task<Result<IReadOnlyList<GastoHabitualDto>>> Handle(GetHabitualesGastosQuery request, CancellationToken cancellationToken)
    {
        if (request.Limit > MaxCacheable)
        {
            var sinCache = await _repository.GetHabitualesAsync(request.UsuarioId, request.Limit, cancellationToken);
            return Result.Success(sinCache);
        }

        var cacheKey = CacheKey(request.UsuarioId);
        var cached = await _cacheService.GetAsync<List<GastoHabitualDto>>(cacheKey);

        if (cached is null)
        {
            var habituales = await _repository.GetHabitualesAsync(request.UsuarioId, MaxCacheable, cancellationToken);
            cached = habituales.ToList();
            await _cacheService.SetAsync(cacheKey, cached, absoluteExpiration: CacheTtl);
        }

        return Result.Success<IReadOnlyList<GastoHabitualDto>>(cached.Take(request.Limit).ToList());
    }
}
