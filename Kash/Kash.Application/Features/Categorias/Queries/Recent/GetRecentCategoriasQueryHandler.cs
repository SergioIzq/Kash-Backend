using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
