using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;

/// <summary>
/// Handler base para consultas GetById.
/// Implementa el patrón "Cache-Aside": Primero busca en caché, luego en BD.
/// </summary>
public abstract class GetByIdQueryHandler<TEntity, TId, TDto, TQuery>
    : AbsQueryHandler<TEntity, TId>, IRequestHandler<TQuery, Result<TDto>>
    where TEntity : AbsEntity<TId>
    where TDto : class
    where TQuery : AbsGetByIdQuery<TEntity, TId, TDto>
    where TId : IGuidValueObject
{
    // Solo usamos el repo de lectura optimizado (Dapper/SQL directo)
    protected readonly IReadRepository<TEntity, TDto, TId> _dtoRepository;

    protected GetByIdQueryHandler(
        IReadRepository<TEntity, TDto, TId> dtoRepository,
        ICacheService cacheService)
        : base(cacheService)
    {
        _dtoRepository = dtoRepository;
    }

    public async Task<Result<TDto>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        // 1. Intentar obtener del cache
        string cacheKey = $"{typeof(TEntity).Name}:{query.Id}";
        var cachedDto = await _cacheService.GetAsync<TDto>(cacheKey);

        if (cachedDto != null)
        {
            return Result.Success(cachedDto);
        }

        // 2. Si no está en caché, ir a la Base de Datos
        // NOTA: Si la BD explota (TimeOut, Conexión), el Middleware Global lo atrapa (500).
        var dto = await _dtoRepository.GetReadModelByIdAsync(query.Id, cancellationToken);

        // 3. Verificación de existencia (Regla de Negocio de Lectura)
        if (dto is null)
        {
            // Usamos tu nuevo Error.NotFound que ya tiene ErrorType.NotFound integrado.
            // Esto hará que el Controller devuelva automáticamente un 404.
            return Result.Failure<TDto>(
                Error.NotFound($"No se encontró el registro de {typeof(TEntity).Name} con ID '{query.Id}'.")
            );
        }

        // 4. Guardar en caché (Cache-Aside pattern)
        await _cacheService.SetAsync(
            cacheKey,
            dto,
            absoluteExpiration: TimeSpan.FromMinutes(30)
        );

        return Result.Success(dto);
    }
}