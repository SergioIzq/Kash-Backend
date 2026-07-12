using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos;

namespace Kash.Application.Features.Dashboard.Queries;

/// <summary>
/// Query para obtener el resumen del dashboard de un usuario con filtros opcionales.
/// </summary>
public sealed record GetDashboardResumenQuery(
    Guid UsuarioId,
 DateTime? FechaInicio = null,
    DateTime? FechaFin = null,
    Guid? CuentaId = null,
    Guid? CategoriaId = null
) : IQuery<DashboardResumenDto>
{
    /// <summary>
    /// Indica si se debe usar el período del mes actual (cuando FechaInicio y FechaFin son null).
    /// </summary>
    public bool UsarMesActual => FechaInicio == null && FechaFin == null;
}
