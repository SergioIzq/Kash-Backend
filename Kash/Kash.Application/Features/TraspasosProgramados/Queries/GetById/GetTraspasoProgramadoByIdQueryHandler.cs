using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
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
