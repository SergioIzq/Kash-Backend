using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Ingresos.Queries;

public sealed class GetIngresosPagedListQueryHandler
    : GetPagedListQueryHandler<Ingreso, IngresoId, IngresoDto, GetIngresosPagedListQuery>
{
    public GetIngresosPagedListQueryHandler(
        IReadRepository<Ingreso, IngresoDto, IngresoId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
