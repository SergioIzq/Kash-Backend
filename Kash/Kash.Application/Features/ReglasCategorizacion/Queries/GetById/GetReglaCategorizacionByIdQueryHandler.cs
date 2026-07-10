using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
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
