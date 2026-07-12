using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
