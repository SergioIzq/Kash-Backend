using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces.Repositories;

/// <summary>
/// Consulta de reglas de auto-categorización para aplicarlas durante la importación.
/// Vive en Application (no en Domain) porque devuelve un DTO de Kash.Shared.Application,
/// al que la capa de Dominio no puede referenciar.
/// </summary>
public interface IReglaCategorizacionReadRepository
{
    /// <summary>
    /// Devuelve las reglas activas del usuario, ordenadas por prioridad (ascendente)
    /// y luego por fecha de creación, listas para aplicar durante la importación.
    /// </summary>
    Task<IEnumerable<ReglaCategorizacionDto>> GetActivasOrdenadasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}
