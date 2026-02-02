using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Gastos.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Gasto.
/// </summary>
public sealed class GetGastoByIdQueryHandler
    : GetByIdQueryHandler<Gasto, GastoId, GastoDto, GetGastoByIdQuery>
{
    public GetGastoByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<Gasto, GastoDto, GastoId> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}