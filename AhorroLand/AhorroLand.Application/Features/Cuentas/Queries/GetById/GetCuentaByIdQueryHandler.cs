using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Cuentas.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Cuenta.
/// </summary>
public sealed class GetCuentaByIdQueryHandler
    : GetByIdQueryHandler<Cuenta, CuentaDto, GetCuentaByIdQuery>
{
    public GetCuentaByIdQueryHandler(
        ICacheService cacheService,
        IReadRepositoryWithDto<Cuenta, CuentaDto> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}