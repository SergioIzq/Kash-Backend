using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Categoria.
/// </summary>
public sealed class GetCategoriaByIdQueryHandler
    : GetByIdQueryHandler<Categoria, CategoriaId, CategoriaDto, GetCategoriaByIdQuery>
{
    public GetCategoriaByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<Categoria, CategoriaDto, CategoriaId> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}
