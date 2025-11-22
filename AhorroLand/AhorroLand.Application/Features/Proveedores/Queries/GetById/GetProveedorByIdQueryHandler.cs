using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Proveedores.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Proveedor.
/// </summary>
public sealed class GetProveedorByIdQueryHandler
    : GetByIdQueryHandler<Proveedor, ProveedorDto, GetProveedorByIdQuery>
{
    public GetProveedorByIdQueryHandler(
        ICacheService cacheService,
        IReadRepositoryWithDto<Proveedor, ProveedorDto> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}