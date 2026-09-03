using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Listado paginado de Gastos de un usuario cuya fecha de transacción cae dentro de un rango
/// indicado. Mismo patrón SQL que <see cref="IGastoExportRepository"/>, pero paginado en vez de
/// devolver el conjunto completo.
/// </summary>
public interface IGastoPeriodoRepository
{
    Task<PeriodoResult<GastoDto>> GetPagedByPeriodoAsync(
        Guid usuarioId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
