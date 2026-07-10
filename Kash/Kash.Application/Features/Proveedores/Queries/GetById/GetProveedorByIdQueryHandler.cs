using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
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