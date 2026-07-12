using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Queries;

/// <summary>
/// Query para búsqueda rápida de clientes (autocomplete).
/// </summary>
public sealed record SearchCategoriasQuery : SearchForAutocompleteQuery<Categoria, CategoriaDto, CategoriaId>
{
    public SearchCategoriasQuery(string searchTerm, int limit = 10)
    : base(searchTerm, limit)
    {
    }
}
