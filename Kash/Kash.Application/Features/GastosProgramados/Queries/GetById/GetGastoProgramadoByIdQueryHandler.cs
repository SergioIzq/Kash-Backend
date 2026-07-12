using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
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
