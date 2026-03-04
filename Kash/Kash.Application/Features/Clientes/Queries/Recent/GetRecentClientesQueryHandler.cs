using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
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
