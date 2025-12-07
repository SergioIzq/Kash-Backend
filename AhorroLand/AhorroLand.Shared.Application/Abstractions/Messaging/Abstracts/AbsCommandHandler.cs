using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Interfaces;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts;

/// <summary>
/// Proporciona métodos base para manejar comandos de escritura (CRUD: C, U, D) de forma asíncrona.
/// Utiliza IWriteRepository e IUnitOfWork para asegurar la segregación de responsabilidades.
/// 🔥 Sistema de versionado de caché para invalidación eficiente de listas.
/// </summary>
/// <typeparam name="TEntity">El tipo de entidad que el command handler manipula, debe heredar de AbsEntity.</typeparam>
public abstract class AbsCommandHandler<TEntity, TId> : IAbsCommandHandlerBase<TEntity, TId>
    where TEntity : AbsEntity<TId>
    where TId : IGuidValueObject
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IWriteRepository<TEntity, TId> _writeRepository;
    protected readonly ICacheService _cacheService;
    protected readonly IUserContext _userContext;
    protected readonly ILogger? _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase AbsCommandHandler.
    /// </summary>
    /// <param name="unitOfWork">La unidad de trabajo para gestionar la persistencia de cambios.</param>
    /// <param name="writeRepository">El repositorio con métodos de escritura (Add, Update, Delete).</param>
    /// <param name="cacheService">Servicio de caché para invalidación.</param>
    /// <param name="userContext">Contexto del usuario actual.</param>
    /// <param name="logger">Logger opcional para debugging.</param>
    public AbsCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<TEntity, TId> writeRepository,
        ICacheService cacheService,
     IUserContext userContext,
ILogger? logger = null)
    {
        _unitOfWork = unitOfWork;
        _writeRepository = writeRepository;
        _cacheService = cacheService;
        _userContext = userContext;
     _logger = logger;
    }

    // --- Métodos CUD (Create, Update, Delete) ---

    /// <summary>
  /// Añade la entidad al repositorio y persiste los cambios.
    /// </summary>
    /// <param name="entity">La entidad a crear.</param>
    /// <param name="cancellationToken">Token para monitorear peticiones de cancelación.</param>
    /// <returns>Un Result que contiene la entidad creada en caso de éxito, o Error.Conflict si falla.</returns>
    public async Task<Result<Guid>> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _writeRepository.Add(entity);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 🔥 Invalidar caché con sistema de versionado
        await InvalidateCacheAsync(entity.Id.Value);

        return Result.Success(entity.Id.Value);
    }

    /// <summary>
    /// Marca la entidad como modificada y persiste los cambios.
    /// </summary>
    /// <param name="entity">La entidad a actualizar.</param>
    /// <param name="cancellationToken">Token para monitorear peticiones de cancelación.</param>
    /// <returns>Un Result de éxito si la actualización fue exitosa, o Error.UpdateFailure si falla.</returns>
    public async Task<Result<Guid>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
      _writeRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    // 🔥 Invalidar caché con sistema de versionado
        await InvalidateCacheAsync(entity.Id.Value);

        return Result.Success(entity.Id.Value);
    }

    /// <summary>
    /// Marca la entidad para su eliminación y persiste los cambios.
    /// </summary>
    /// <param name="entity">La entidad a eliminar.</param>
    /// <param name="cancellationToken">Token para monitorear peticiones de cancelación.</param>
    /// <returns>Un Result de éxito si la eliminación fue exitosa, o Error.DeleteFailure si falla.</returns>
    public async Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _writeRepository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 🔥 Invalidar caché con sistema de versionado
        await InvalidateCacheAsync(entity.Id.Value);

        return Result.Success();
    }

    /// <summary>
    /// 🔥 NUEVO: Invalida caché usando sistema de versionado por usuario.
    /// Invalida: caché individual de la entidad + versión de lista del usuario.
    /// </summary>
    protected async Task InvalidateCacheAsync(Guid id)
    {
        var entityName = typeof(TEntity).Name;
 
        // 1. Invalidar caché individual de la entidad
        var individualKey = $"{entityName}:{id}";
        await _cacheService.RemoveAsync(individualKey);
   
        _logger?.LogInformation("🗑️ Caché individual invalidado: {CacheKey}", individualKey);
        
   // 2. 🔥 Invalidar versión de lista del usuario
      // Esto fuerza a que todas las queries de lista/paginación se recalculen
        if (_userContext.UserId.HasValue)
  {
            var versionKey = $"list_version:{entityName}:{_userContext.UserId}";
            await _cacheService.RemoveAsync(versionKey);
      
            _logger?.LogInformation("🗑️ Versión de lista invalidada: {VersionKey} para usuario {UserId}", 
         versionKey, _userContext.UserId);
        }
        else
        {
 _logger?.LogWarning("⚠️ No se pudo invalidar caché de lista porque UserId es null");
        }
    }
}