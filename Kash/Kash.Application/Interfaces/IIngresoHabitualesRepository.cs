using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Equivalente de <see cref="IGastoHabitualesRepository"/> para ingresos.
/// </summary>
public interface IIngresoHabitualesRepository
{
    /// <summary>
    /// Devuelve hasta <paramref name="limit"/> combinaciones, ordenadas por nº de veces (desc.)
    /// y fecha de último uso (desc.). Solo incluye combinaciones repetidas al menos dos veces.
    /// </summary>
    Task<IReadOnlyList<IngresoHabitualDto>> GetHabitualesAsync(Guid usuarioId, int limit, CancellationToken cancellationToken = default);
}
