using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.GastosProgramados.Queries;

public sealed class GetGastoProgramadoByIdQueryHandler
    : GetByIdQueryHandler<GastoProgramado, GastoProgramadoDto, GetGastoProgramadoByIdQuery>
{
    public GetGastoProgramadoByIdQueryHandler(
  ICacheService cacheService,
      IReadRepositoryWithDto<GastoProgramado, GastoProgramadoDto> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}
