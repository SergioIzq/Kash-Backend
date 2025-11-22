using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using MediatR;

namespace AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;

/// <summary>
/// Handler base para consultas GetById.
/// ✅ OPTIMIZADO: Usa IReadRepositoryWithDto para obtener DTOs directamente desde SQL.
/// </summary>
public abstract class GetByIdQueryHandler<TEntity, TDto, TQuery>
 : AbsQueryHandler<TEntity>, IRequestHandler<TQuery, Result<TDto>>
    where TEntity : AbsEntity
    where TDto : class
  where TQuery : AbsGetByIdQuery<TEntity, TDto>
{
    // 🔥 ÚNICO REPOSITORIO: Solo usamos IReadRepositoryWithDto
    protected readonly IReadRepositoryWithDto<TEntity, TDto> _dtoRepository;

    // 🔥 Constructor simplificado
    public GetByIdQueryHandler(
     IReadRepositoryWithDto<TEntity, TDto> dtoRepository,
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

        // 2. 🚀 OPTIMIZADO: Obtener DTO directamente desde SQL sin mapeo
        var dto = await _dtoRepository.GetReadModelByIdAsync(query.Id, cancellationToken);

        if (dto is null)
        {
            return Result.Failure<TDto>(Error.NotFound(
           $"Entidad con ID '{query.Id}' de tipo {typeof(TEntity).Name} no fue encontrada."));
        }

        // 3. Cachear el DTO
        await _cacheService.SetAsync(
      cacheKey,
            dto,
            absoluteExpiration: TimeSpan.FromMinutes(30)
        );

        return Result.Success(dto);
    }
}