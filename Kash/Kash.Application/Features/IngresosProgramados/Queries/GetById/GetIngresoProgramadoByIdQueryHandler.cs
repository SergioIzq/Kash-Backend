using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.IngresosProgramados.Queries;

public sealed class GetIngresoProgramadoByIdQueryHandler
    : GetByIdQueryHandler<IngresoProgramado, IngresoProgramadoId, IngresoProgramadoDto, GetIngresoProgramadoByIdQuery>
{
    public GetIngresoProgramadoByIdQueryHandler(
  IReadRepository<IngresoProgramado, IngresoProgramadoDto, IngresoProgramadoId> repository,
    ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
