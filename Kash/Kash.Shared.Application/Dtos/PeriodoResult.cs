using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Envuelve un listado paginado por periodo junto con la suma del <c>Importe</c> de
    /// TODOS los registros que cumplen el filtro de fecha (no solo los de la página actual).
    /// </summary>
    public sealed record PeriodoResult<T>(PagedList<T> Pagina, decimal SumaImporte);
}
