using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Equivalente de <see cref="IGastoSugerenciaRepository"/> para ingresos.
/// </summary>
public interface IIngresoSugerenciaRepository
{
    /// <summary>
    /// Último ingreso del usuario para ese concepto (por fecha de transacción, desempatando por
    /// fecha de creación si coincide la fecha), o <c>null</c> si no hay histórico.
    /// </summary>
    Task<IngresoDto?> GetUltimoUsoAsync(Guid usuarioId, Guid conceptoId, CancellationToken cancellationToken = default);
}
