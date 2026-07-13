using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos.Reportes;

namespace Kash.Application.Features.Reportes.Queries.GetPresupuestoExcel;

/// <summary>
/// Genera el reporte Excel de presupuesto financiero del usuario para el rango de fechas indicado.
/// </summary>
public sealed record GetPresupuestoExcelQuery(
    Guid UsuarioId,
    DateTime FechaInicio,
    DateTime FechaFin
) : IQuery<PresupuestoArchivoDto>;
