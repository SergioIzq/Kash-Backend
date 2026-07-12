using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Proveedores.Queries.Recent;

public sealed class GetRecentProveedoresQueryHandler
    : GetRecentQueryHandler<Proveedor, ProveedorDto, ProveedorId, GetRecentProveedoresQuery>
{
    public GetRecentProveedoresQueryHandler(
      IReadRepository<Proveedor, ProveedorDto, ProveedorId> repository,
ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
