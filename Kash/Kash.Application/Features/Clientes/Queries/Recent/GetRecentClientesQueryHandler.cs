using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Clientes.Queries.Recent;

public sealed class GetRecentClientesQueryHandler
  : GetRecentQueryHandler<Cliente, ClienteDto, ClienteId, GetRecentClientesQuery>
{
    public GetRecentClientesQueryHandler(
        IReadRepository<Cliente, ClienteDto, ClienteId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
