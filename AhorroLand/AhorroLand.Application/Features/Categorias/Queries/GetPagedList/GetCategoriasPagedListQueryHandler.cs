using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Categorias.Queries;

/// <summary>
/// Manejador concreto para la consulta de lista paginada de Categorías.
/// Implementa la lógica específica de filtrado y ordenación.
/// </summary>
public sealed class GetCategoriasPagedListQueryHandler
    : GetPagedListQueryHandler<Categoria, CategoriaDto, GetCategoriasPagedListQuery>
{
    public GetCategoriasPagedListQueryHandler(
        IReadRepositoryWithDto<Categoria, CategoriaDto> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}