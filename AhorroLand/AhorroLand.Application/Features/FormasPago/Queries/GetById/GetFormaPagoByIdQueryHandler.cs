using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.FormasPago.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad FormaPago.
/// </summary>
public sealed class GetFormaPagoByIdQueryHandler
    : GetByIdQueryHandler<FormaPago, FormaPagoDto, GetFormaPagoByIdQuery>
{
    public GetFormaPagoByIdQueryHandler(
        ICacheService cacheService,
        IReadRepositoryWithDto<FormaPago, FormaPagoDto> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}