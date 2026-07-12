using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.TraspasosProgramados.Queries;

public sealed class GetTraspasoProgramadoByIdQueryHandler
    : GetByIdQueryHandler<TraspasoProgramado, TraspasoProgramadoId, TraspasoProgramadoDto, GetTraspasoProgramadoByIdQuery>
{
    public GetTraspasoProgramadoByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId> readOnlyRepository
    )
    : base(readOnlyRepository, cacheService)
    {
    }
}
