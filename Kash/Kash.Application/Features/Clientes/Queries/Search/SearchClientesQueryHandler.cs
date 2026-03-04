using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Clientes.Queries.Search;

/// <summary>
/// Handler para búsqueda rápida de clientes (autocomplete).
/// </summary>
public sealed class SearchClientesQueryHandler
    : SearchForAutocompleteQueryHandler<Cliente, ClienteDto, SearchClientesQuery, ClienteId>
{
    public SearchClientesQueryHandler(
        IReadRepository<Cliente, ClienteDto, ClienteId> repository,
   ICacheService cacheService)
  : base(repository, cacheService)
    {
    }
}
