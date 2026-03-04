using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using MediatR;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;

public abstract class SearchForAutocompleteQueryHandler<TEntity, TDto, TQuery, TId>
    : AbsQueryHandler<TEntity, TId>, IRequestHandler<TQuery, Result<IEnumerable<TDto>>>
    where TEntity : AbsEntity<TId>
    where TQuery : SearchForAutocompleteQuery<TEntity, TDto, TId>
    where TDto : class
    where TId : IGuidValueObject
{
    protected readonly IReadRepository<TEntity, TDto, TId> _repository;

    protected SearchForAutocompleteQueryHandler(
        IReadRepository<TEntity, TDto, TId> repository,
        ICacheService cacheService)
        : base(cacheService)
    {
        _repository = repository;
    }

    // 🔥 1. Hook virtual para filtros personalizados (igual que en GetRecent)
    protected virtual Dictionary<string, object>? GetCustomFilters(TQuery query)
    {
        return null;
    }

    public virtual async Task<Result<IEnumerable<TDto>>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            return Result.Success(Enumerable.Empty<TDto>());
        }

        if (!query.UsuarioId.HasValue)
        {
            return Result.Failure<IEnumerable<TDto>>(
                Error.Validation("El ID de usuario es requerido para la búsqueda."));
        }

        // 🔥 2. Obtener filtros personalizados
        var extraFilters = GetCustomFilters(query);

        // 🔥 3. Pasar extraFilters al repositorio
        var results = await _repository.SearchForAutocompleteAsync(
            query.UsuarioId.Value,
            query.SearchTerm,
            query.Limit,
            extraFilters, // <--- Nuevo parámetro
            cancellationToken);

        return Result.Success(results);
    }
}