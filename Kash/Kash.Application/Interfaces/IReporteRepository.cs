using Kash.Shared.Application.Dtos.Reportes;

namespace Kash.Application.Interfaces;

/// <summary>
/// Repositorio de lectura (Dapper) para los reportes financieros agregados.
/// </summary>
public interface IReporteRepository
{
    /// <summary>
    /// Agrega los movimientos del usuario en el rango [fechaInicio, fechaFin] en la estructura
    /// de un presupuesto: totales, ingresos y gastos por categoría → concepto, y desgloses por
    /// cuenta y forma de pago.
    /// </summary>
    Task<PresupuestoReportDto> GetPresupuestoAsync(
        Guid usuarioId,
        DateTime fechaInicio,
        DateTime fechaFin,
        CancellationToken cancellationToken = default);
}
