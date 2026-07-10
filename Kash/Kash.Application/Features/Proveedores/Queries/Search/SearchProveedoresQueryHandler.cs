using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
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
