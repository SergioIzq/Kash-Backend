using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
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
