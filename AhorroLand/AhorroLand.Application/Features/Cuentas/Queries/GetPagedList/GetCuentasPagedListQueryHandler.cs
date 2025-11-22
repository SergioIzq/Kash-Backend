using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Cuentas.Queries;

public sealed class GetCuentasPagedListQueryHandler
    : GetPagedListQueryHandler<Cuenta, CuentaDto, GetCuentasPagedListQuery>
{
    public GetCuentasPagedListQueryHandler(
     IReadRepositoryWithDto<Cuenta, CuentaDto> repository,
  ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}