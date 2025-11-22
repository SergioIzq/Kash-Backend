using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.GastosProgramados.Queries;

public sealed class GetGastosProgramadosPagedListQueryHandler
  : GetPagedListQueryHandler<GastoProgramado, GastoProgramadoDto, GetGastosProgramadosPagedListQuery>
{
 public GetGastosProgramadosPagedListQueryHandler(
   IReadRepositoryWithDto<GastoProgramado, GastoProgramadoDto> repository,
 ICacheService cacheService)
      : base(repository, cacheService)
    {
    }
}
