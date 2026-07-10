using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using SergioIzq.Application.Kernel.Services;
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
