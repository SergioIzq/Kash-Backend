using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.FormasPago.Queries;

/// <summary>
/// Maneja la creación de una nueva entidad FormaPago.
/// </summary>
public sealed class GetFormaPagoByIdQueryHandler
    : GetByIdQueryHandler<FormaPago, FormaPagoId, FormaPagoDto, GetFormaPagoByIdQuery>
{
    public GetFormaPagoByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<FormaPago, FormaPagoDto, FormaPagoId> readOnlyRepository
        )
        : base(readOnlyRepository, cacheService)
    {
    }
}