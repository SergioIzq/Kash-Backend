using Kash.Application.Interfaces;
using Kash.Shared.Application.Dtos;
using MediatR;
using SergioIzq.Application.Kernel.Services;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Ingresos.Queries.Habituales;

/// <summary>
/// Equivalente de <see cref="Gastos.Queries.Habituales.GetHabitualesGastosQueryHandler"/> para ingresos.
/// Mismo esquema de caché: clave única por usuario (sin sufijo `limit`), invalidable con
/// `RemoveAsync` exacto desde `CreateIngresoCommandHandler.OnEntityCreatedAsync`.
/// </summary>
public sealed class GetHabitualesIngresosQueryHandler
    : IRequestHandler<GetHabitualesIngresosQuery, Result<IReadOnlyList<IngresoHabitualDto>>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
    private const int MaxCacheable = 20;

    private readonly IIngresoHabitualesRepository _repository;
    private readonly ICacheService _cacheService;

    public GetHabitualesIngresosQueryHandler(IIngresoHabitualesRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public static string CacheKey(Guid usuarioId) => $"ingreso_habituales:{usuarioId}";

    public async Task<Result<IReadOnlyList<IngresoHabitualDto>>> Handle(GetHabitualesIngresosQuery request, CancellationToken cancellationToken)
    {
        if (request.Limit > MaxCacheable)
        {
            var sinCache = await _repository.GetHabitualesAsync(request.UsuarioId, request.Limit, cancellationToken);
            return Result.Success(sinCache);
        }

        var cacheKey = CacheKey(request.UsuarioId);
        var cached = await _cacheService.GetAsync<List<IngresoHabitualDto>>(cacheKey);

        if (cached is null)
        {
            var habituales = await _repository.GetHabitualesAsync(request.UsuarioId, MaxCacheable, cancellationToken);
            cached = habituales.ToList();
            await _cacheService.SetAsync(cacheKey, cached, absoluteExpiration: CacheTtl);
        }

        return Result.Success<IReadOnlyList<IngresoHabitualDto>>(cached.Take(request.Limit).ToList());
    }
}
