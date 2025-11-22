using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Ingresos.Queries;

public sealed class GetIngresosPagedListQueryHandler
    : GetPagedListQueryHandler<Ingreso, IngresoDto, GetIngresosPagedListQuery>
{
    public GetIngresosPagedListQueryHandler(
        IReadRepositoryWithDto<Ingreso, IngresoDto> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
