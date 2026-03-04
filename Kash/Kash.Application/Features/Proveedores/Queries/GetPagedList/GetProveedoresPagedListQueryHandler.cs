using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
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