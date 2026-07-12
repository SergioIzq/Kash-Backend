using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Proveedores.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Proveedor.
/// </summary>
public sealed class GetProveedorByIdQueryHandler
    : GetByIdQueryHandler<Proveedor, ProveedorId, ProveedorDto, GetProveedorByIdQuery>
{
    public GetProveedorByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<Proveedor, ProveedorDto, ProveedorId> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}
