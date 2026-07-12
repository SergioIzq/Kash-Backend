using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
