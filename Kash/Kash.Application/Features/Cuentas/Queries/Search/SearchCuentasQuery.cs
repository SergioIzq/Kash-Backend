using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Queries;

/// <summary>
/// Query para búsqueda rápida de cuentas (autocomplete).
/// </summary>
public sealed record SearchCuentasQuery : SearchForAutocompleteQuery<Cuenta, CuentaDto, CuentaId>
{
    public SearchCuentasQuery(string searchTerm, int limit = 10)
    : base(searchTerm, limit)
    {
    }
}
