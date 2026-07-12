using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Queries;

public sealed class GetReglaCategorizacionByIdQueryHandler
    : GetByIdQueryHandler<ReglaCategorizacion, ReglaCategorizacionId, ReglaCategorizacionDto, GetReglaCategorizacionByIdQuery>
{
    public GetReglaCategorizacionByIdQueryHandler(
        ICacheService cacheService,
        IReadRepository<ReglaCategorizacion, ReglaCategorizacionDto, ReglaCategorizacionId> readOnlyRepository)
        : base(readOnlyRepository, cacheService)
    {
    }
}
