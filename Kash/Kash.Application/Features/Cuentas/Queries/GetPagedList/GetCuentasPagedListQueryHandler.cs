using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Queries;

public sealed class GetCuentasPagedListQueryHandler
    : GetPagedListQueryHandler<Cuenta, CuentaId, CuentaDto, GetCuentasPagedListQuery>
{
    public GetCuentasPagedListQueryHandler(
     IReadRepository<Cuenta, CuentaDto, CuentaId> repository,
  ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}