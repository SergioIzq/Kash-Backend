using Kash.Shared.Application.Abstractions.Servicies;
using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries
{
    /// <summary>
    /// Handler base para consultas paginadas.
    /// OPTIMIZADO: Usa paginación a nivel de base de datos para evitar cargar toda la tabla en memoria.
    /// FIX: Usa GetPagedReadModelsAsync para evitar problemas de mapeo con Value Objects.
    /// CACHE: Implementa cache con Redis para requests repetidos (~5ms en lugar de 370ms).
    /// Sistema de versionado para invalidación automática de listas.
    /// </summary>
    public abstract class GetPagedListQueryHandler<TEntity, TId, TDto, TQuery>
        : AbsQueryHandler<TEntity, TId>, IRequestHandler<TQuery, Result<PagedList<TDto>>>
        where TEntity : AbsEntity<TId>
        where TQuery : AbsGetPagedListQuery<TEntity, TId, TDto>
        where TDto : class
        where TId : IGuidValueObject
    {
        // ÚNICO REPOSITORIO: Solo usamos IReadRepository
        protected readonly IReadRepository<TEntity, TDto, TId> _dtoRepository;

        // Constructor simplificado
        public GetPagedListQueryHandler(
            IReadRepository<TEntity, TDto, TId> dtoRepository,
            ICacheService cacheService)
            : base(cacheService)
        {
            _dtoRepository = dtoRepository;
        }

        /// <summary>
        /// MÉTODO ABSTRACTO OPCIONAL: Permite aplicar filtros adicionales antes de paginar.
        /// Si no se sobrescribe, se usa paginación directa desde el repositorio.
        /// </summary>
        protected virtual Task<PagedList<TDto>> ApplyFiltersAsync(
            TQuery query,
            CancellationToken cancellationToken)
        {
            // Por defecto, no aplica filtros adicionales
            return Task.FromResult<PagedList<TDto>>(null!);
        }

        public virtual async Task<Result<PagedList<TDto>>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            // 1. Obtener versión actual de la lista (se invalida cuando hay CUD)
            string versionKey = $"list_version:{typeof(TEntity).Name}:{query.UsuarioId}";
            string? listVersion = await _cacheService.GetAsync<string>(versionKey);

            // Si no existe versión, crear una nueva
            if (string.IsNullOrEmpty(listVersion))
            {
                listVersion = Guid.NewGuid().ToString();
                // REDUCIDO: TTL más corto para evitar problemas de sincronización (de 5/10 min a 2/3 min)
                await _cacheService.SetAsync(
                    versionKey,
                    listVersion,
                    slidingExpiration: TimeSpan.FromMinutes(2),
                    absoluteExpiration: TimeSpan.FromMinutes(3));
            }

            // 2. Construir clave de caché que incluye la versión
            // MEJORADO: Incluir searchTerm, sortColumn y sortOrder en la clave de caché
            string cacheKey = $"{typeof(TEntity).Name}:paged:{query.UsuarioId}:{listVersion}:{query.Page}:{query.PageSize}:{query.SearchTerm}:{query.SortColumn}:{query.SortOrder}";

            // 3. Intentar obtener de caché
            var cachedResult = await _cacheService.GetAsync<PagedList<TDto>>(cacheKey);

            if (cachedResult != null)
            {
                return Result.Success(cachedResult); // ~5ms desde cache
            }

            // 4. Intentar obtener filtros personalizados del handler concreto
            var customFiltered = await ApplyFiltersAsync(query, cancellationToken);

            if (customFiltered != null)
            {
                // Cachear resultado filtrado con TTL corto
                await _cacheService.SetAsync(
                    cacheKey,
                    customFiltered,
                    slidingExpiration: TimeSpan.FromMinutes(1),
                    absoluteExpiration: TimeSpan.FromMinutes(2));

                return Result.Success(customFiltered);
            }

            // 5. OPTIMIZACIÓN: Usar paginación a nivel de base de datos
            PagedList<TDto> pagedDtos;

            if (query.UsuarioId.HasValue)
            {
                // USA ÍNDICES: Filtrar por usuario (reduce 370ms a ~50ms)
                pagedDtos = await _dtoRepository.GetPagedReadModelsByUserAsync(
                    query.UsuarioId.Value,
                    query.Page,
                    query.PageSize,
                    query.SearchTerm, // searchTerm
                    query.SortColumn, // sortColumn
                    query.SortOrder, // sortOrder
                    cancellationToken);
            }
            else
            {
                // Sin filtro de usuario (menos eficiente)
                pagedDtos = await _dtoRepository.GetPagedReadModelsAsync(
                    query.Page,
                    query.PageSize,
                    cancellationToken);
            }

            // 6. CACHE: Guardar en cache con TTL reducido para mejor consistencia
            await _cacheService.SetAsync(
                cacheKey,
                pagedDtos,
                slidingExpiration: TimeSpan.FromMinutes(1),
                absoluteExpiration: TimeSpan.FromMinutes(2));

            return Result.Success(pagedDtos);
        }
    }
}
