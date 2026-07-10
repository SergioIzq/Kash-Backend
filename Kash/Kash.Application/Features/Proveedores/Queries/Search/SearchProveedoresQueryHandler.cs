using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Proveedores.Queries.Search;

/// <summary>
/// Handler para búsqueda rápida de proveedores (autocomplete).
/// </summary>
public sealed class SearchProveedoresQueryHandler
    : SearchForAutocompleteQueryHandler<Proveedor, ProveedorDto, SearchProveedoresQuery, ProveedorId>
{
    public SearchProveedoresQueryHandler(
    IReadRepository<Proveedor, ProveedorDto, ProveedorId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
