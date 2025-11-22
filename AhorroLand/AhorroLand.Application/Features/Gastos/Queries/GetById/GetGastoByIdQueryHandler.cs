using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.Gastos.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad Gasto.
/// </summary>
public sealed class GetGastoByIdQueryHandler
    : GetByIdQueryHandler<Gasto, GastoDto, GetGastoByIdQuery>
{
    public GetGastoByIdQueryHandler(
        ICacheService cacheService,
        IReadRepositoryWithDto<Gasto, GastoDto> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}