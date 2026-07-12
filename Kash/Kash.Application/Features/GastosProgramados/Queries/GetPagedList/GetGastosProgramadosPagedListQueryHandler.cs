using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.GastosProgramados.Queries;

public sealed class GetGastosProgramadosPagedListQueryHandler
  : GetPagedListQueryHandler<GastoProgramado, GastoProgramadoId, GastoProgramadoDto, GetGastosProgramadosPagedListQuery>
{
    public GetGastosProgramadosPagedListQueryHandler(
      IReadRepository<GastoProgramado, GastoProgramadoDto, GastoProgramadoId> repository,
    ICacheService cacheService)
         : base(repository, cacheService)
    {
    }
}
