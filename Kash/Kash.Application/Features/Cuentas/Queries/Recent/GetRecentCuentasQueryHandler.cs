using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Queries.Recent;

public sealed class GetRecentCuentasQueryHandler
    : GetRecentQueryHandler<Cuenta, CuentaDto, CuentaId, GetRecentCuentasQuery>
{
    public GetRecentCuentasQueryHandler(
   IReadRepository<Cuenta, CuentaDto, CuentaId> repository,
        ICacheService cacheService)
      : base(repository, cacheService)
    {
    }
}
