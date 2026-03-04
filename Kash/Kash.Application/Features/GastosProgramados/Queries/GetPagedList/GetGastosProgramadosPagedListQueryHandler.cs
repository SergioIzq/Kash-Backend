using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
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
