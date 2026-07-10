using Kash.Shared.Application.Abstractions.Servicies;
using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;

/// <summary>
/// Handler base para obtener elementos recientes ultra-r�pidos.
/// ? Con cache de corta duraci�n (30 segundos) para mejorar performance
/// ? Resultados limitados (m�ximo 5-50 items)
/// ? Optimizado para <10ms de respuesta
/// </summary>
public abstract class GetRecentQueryHandler<TEntity, TDto, TId, TQuery>
    : AbsQueryHandler<TEntity, TId>, IRequestHandler<TQuery, Result<IEnumerable<TDto>>>
    where TEntity : AbsEntity<TId>
    where TQuery : GetRecentQuery<TEntity, TDto, TId>
    where TDto : class
    where TId : IGuidValueObject
{
    protected readonly IReadRepository<TEntity, TDto, TId> _repository;

    protected GetRecentQueryHandler(
        IReadRepository<TEntity, TDto, TId> repository,
        ICacheService cacheService)
        : base(cacheService)
    {
        _repository = repository;
    }
    protected virtual Dictionary<string, object>? GetCustomFilters(TQuery query)
    {
        return null; // Por defecto no hay filtros extras
    }
    protected virtual string GetCacheKeySuffix(TQuery query)
    {
        return string.Empty; // Por defecto vac�o
    }

    public virtual async Task<Result<IEnumerable<TDto>>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        if (!query.UsuarioId.HasValue)
        {
            return Result.Failure<IEnumerable<TDto>>(Error.Validation("El ID de usuario es requerido."));
        }

        // Obtener sufijo de cach(ej: ":cat_12345")
        var cacheSuffix = GetCacheKeySuffix(query);

        string cacheKey = $"{typeof(TEntity).Name}:recent:{query.UsuarioId}:{query.Limit}{cacheSuffix}";

        var cachedResult = await _cacheService.GetAsync<IEnumerable<TDto>>(cacheKey);

        if (cachedResult != null)
        {
            return Result.Success(cachedResult);
        }

        // Obtener filtros personalizados (ej: { "c.id_categoria": "123..." })
        var extraFilters = GetCustomFilters(query);

        // Llamar al repositorio con los filtros
        var results = await _repository.GetRecentAsync(
            query.UsuarioId.Value,
            query.Limit,
            extraFilters, // Pasamos los filtros
            cancellationToken);

        await _cacheService.SetAsync(cacheKey, results, slidingExpiration: TimeSpan.FromSeconds(30));

        return Result.Success(results);
    }
}
