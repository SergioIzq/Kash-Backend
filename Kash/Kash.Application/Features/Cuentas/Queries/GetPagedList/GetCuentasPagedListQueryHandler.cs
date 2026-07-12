using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
