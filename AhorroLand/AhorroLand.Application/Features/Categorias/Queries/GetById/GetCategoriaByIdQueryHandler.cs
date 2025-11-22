using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Categorias.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Categoria.
/// </summary>
public sealed class GetCategoriaByIdQueryHandler
    : GetByIdQueryHandler<Categoria, CategoriaDto, GetCategoriaByIdQuery>
{
    public GetCategoriaByIdQueryHandler(
        ICacheService cacheService,
        IReadRepositoryWithDto<Categoria, CategoriaDto> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}