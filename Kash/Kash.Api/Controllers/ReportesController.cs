using Kash.Application.Features.Reportes.Queries.GetPresupuestoExcel;
using Kash.Application.Features.Reportes.Queries.GetPresupuestoPdf;
using SergioIzq.AspNetCore.Kernel.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

/// <summary>
/// Reportes financieros descargables del usuario.
/// </summary>
[Authorize]
[ApiController]
[Route("api/reportes")]
public class ReportesController : AbsController
{
    public ReportesController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Genera y descarga el reporte PDF de presupuesto financiero para el rango de fechas indicado.
    /// </summary>
    [HttpGet("presupuesto/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPresupuestoPdf(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        CancellationToken cancellationToken)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetPresupuestoPdfQuery(usuarioId, fechaInicio, fechaFin);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return File(result.Value.Contenido, "application/pdf", result.Value.NombreArchivo);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Genera y descarga el reporte Excel de presupuesto financiero para el rango de fechas indicado.
    /// </summary>
    [HttpGet("presupuesto/excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPresupuestoExcel(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        CancellationToken cancellationToken)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetPresupuestoExcelQuery(usuarioId, fechaInicio, fechaFin);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return File(
                result.Value.Contenido,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                result.Value.NombreArchivo);
        }

        return HandleResult(result);
    }
}
