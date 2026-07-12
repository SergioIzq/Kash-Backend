using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Queries;

/// <summary>
/// Handler para búsqueda rápida de clientes (autocomplete).
/// </summary>
public sealed class SearchCategoriasQueryHandler
    : SearchForAutocompleteQueryHandler<Categoria, CategoriaDto, SearchCategoriasQuery, CategoriaId>
{
    public SearchCategoriasQueryHandler(
        IReadRepository<Categoria, CategoriaDto, CategoriaId> repository,
   ICacheService cacheService)
  : base(repository, cacheService)
    {
    }
}
