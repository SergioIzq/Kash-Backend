using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Combinaciones completas de gasto (concepto/categoría/cuenta/forma de pago/proveedor/persona)
/// más repetidas por un usuario. Vive en Application (no en Domain, como el resto de repositorios
/// de lectura) porque su resultado es un agregado expuesto como DTO, no una entidad de dominio,
/// mismo patrón que <see cref="IDashboardRepository"/>.
/// </summary>
public interface IGastoHabitualesRepository
{
    /// <summary>
    /// Devuelve hasta <paramref name="limit"/> combinaciones, ordenadas por nº de veces (desc.)
    /// y fecha de último uso (desc.). Solo incluye combinaciones repetidas al menos dos veces.
    /// </summary>
    Task<IReadOnlyList<GastoHabitualDto>> GetHabitualesAsync(Guid usuarioId, int limit, CancellationToken cancellationToken = default);
}
