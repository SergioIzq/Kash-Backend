using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Categorias.Queries.Recent;

/// <summary>
/// Query para obtener categorías recientes.
/// </summary>
public sealed record GetRecentCategoriasQuery : GetRecentQuery<Categoria, CategoriaDto, CategoriaId>
{
    public GetRecentCategoriasQuery(int limit = 5)
      : base(limit)
    {
    }
}
