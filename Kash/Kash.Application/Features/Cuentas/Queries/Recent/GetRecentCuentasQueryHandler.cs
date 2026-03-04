using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
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
