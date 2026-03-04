using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Cuenta.
/// </summary>
public sealed class GetCuentaByIdQueryHandler
    : GetByIdQueryHandler<Cuenta, CuentaId, CuentaDto, GetCuentaByIdQuery>
{
    public GetCuentaByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<Cuenta, CuentaDto, CuentaId> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}