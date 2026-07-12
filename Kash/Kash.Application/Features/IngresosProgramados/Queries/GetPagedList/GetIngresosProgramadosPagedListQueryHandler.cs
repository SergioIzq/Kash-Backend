using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.IngresosProgramados.Queries;

public sealed class GetIngresosProgramadosPagedListQueryHandler
    : GetPagedListQueryHandler<IngresoProgramado, IngresoProgramadoId, IngresoProgramadoDto, GetIngresosProgramadosPagedListQuery>
{
    public GetIngresosProgramadosPagedListQueryHandler(
        IReadRepository<IngresoProgramado, IngresoProgramadoDto, IngresoProgramadoId> repository,
     ICacheService cacheService)
   : base(repository, cacheService)
    {
    }
}
