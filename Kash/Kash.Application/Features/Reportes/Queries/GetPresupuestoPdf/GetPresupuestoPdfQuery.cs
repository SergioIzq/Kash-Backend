using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos.Reportes;

namespace Kash.Application.Features.Reportes.Queries.GetPresupuestoPdf;

/// <summary>
/// Genera el reporte PDF de presupuesto financiero del usuario para el rango de fechas indicado.
/// </summary>
public sealed record GetPresupuestoPdfQuery(
    Guid UsuarioId,
    DateTime FechaInicio,
    DateTime FechaFin
) : IQuery<PresupuestoArchivoDto>;
