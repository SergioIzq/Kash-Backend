using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Proveedores.Queries;

public sealed class GetProveedoresPagedListQueryHandler
    : GetPagedListQueryHandler<Proveedor, ProveedorId, ProveedorDto, GetProveedoresPagedListQuery>
{
    public GetProveedoresPagedListQueryHandler(
        IReadRepository<Proveedor, ProveedorDto, ProveedorId> repository,
     ICacheService cacheService)
  : base(repository, cacheService)
    {
    }
}
