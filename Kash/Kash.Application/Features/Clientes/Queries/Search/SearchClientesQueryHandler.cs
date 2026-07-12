using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
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
