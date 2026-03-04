using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Queries.Recent;

/// <summary>
/// Handler para obtener categorías recientes.
/// </summary>
public sealed class GetRecentCategoriasQueryHandler
    : GetRecentQueryHandler<Categoria, CategoriaDto, CategoriaId, GetRecentCategoriasQuery>
{
    public GetRecentCategoriasQueryHandler(
        IReadRepository<Categoria, CategoriaDto, CategoriaId> repository,
      ICacheService cacheService)
      : base(repository, cacheService)
    {
    }
}
