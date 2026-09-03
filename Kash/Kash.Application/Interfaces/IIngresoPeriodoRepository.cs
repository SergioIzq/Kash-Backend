using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Listado paginado de Ingresos de un usuario cuya fecha de transacción cae dentro de un rango
/// indicado. Mismo patrón SQL que <see cref="IIngresoExportRepository"/>, pero paginado en vez de
/// devolver el conjunto completo.
/// </summary>
public interface IIngresoPeriodoRepository
{
    Task<PeriodoResult<IngresoDto>> GetPagedByPeriodoAsync(
        Guid usuarioId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
