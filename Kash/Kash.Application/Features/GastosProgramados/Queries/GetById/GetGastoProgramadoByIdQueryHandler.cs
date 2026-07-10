using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.GastosProgramados.Queries;

public sealed class GetGastoProgramadoByIdQueryHandler
    : GetByIdQueryHandler<GastoProgramado, GastoProgramadoId, GastoProgramadoDto, GetGastoProgramadoByIdQuery>
{
    public GetGastoProgramadoByIdQueryHandler(
  ICacheService cacheService,
      IReadRepository<GastoProgramado, GastoProgramadoDto, GastoProgramadoId> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}
