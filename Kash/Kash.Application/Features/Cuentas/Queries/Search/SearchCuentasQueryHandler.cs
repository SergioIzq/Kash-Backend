using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Queries.Search;

/// <summary>
/// Handler para búsqueda rápida de cuentas (autocomplete).
/// </summary>
public sealed class SearchCuentasQueryHandler
    : SearchForAutocompleteQueryHandler<Cuenta, CuentaDto, SearchCuentasQuery, CuentaId>
{
    public SearchCuentasQueryHandler(
    IReadRepository<Cuenta, CuentaDto, CuentaId> repository,
        ICacheService cacheService)
        : base(repository, cacheService)
    {
    }
}
