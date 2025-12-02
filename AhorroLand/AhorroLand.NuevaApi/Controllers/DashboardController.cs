using AhorroLand.Application.Features.Dashboard.Queries;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Result y Error
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

/// <summary>
/// Controller para el dashboard con métricas y resumen del usuario.
/// </summary>
[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : AbsController
{
    public DashboardController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene el resumen completo del dashboard.
    /// Cacheado por 1 minuto para evitar sobrecarga en base de datos.
    /// </summary>
    [HttpGet("resumen")]
    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "fechaInicio", "fechaFin", "cuentaId", "categoriaId" })]
    [ProducesResponseType(typeof(DashboardResumenDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] Guid? cuentaId = null,
        [FromQuery] Guid? categoriaId = null)
    {
        // 1. Obtener Usuario (Helper base)
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Token inválido o usuario no identificado.")));
        }

        // 2. Validación de fechas (Regla de presentación/api)
        // Usamos Result.Failure con Error.Validation para mantener el formato estándar
        if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin < fechaInicio)
        {
            return BadRequest(Result.Failure(Error.Validation("La fecha de fin debe ser posterior a la fecha de inicio.")));
        }

        var query = new GetDashboardResumenQuery(
            usuarioId.Value,
            fechaInicio,
            fechaFin,
            cuentaId,
            categoriaId);

        var result = await _sender.Send(query);

        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene el histórico de los últimos N meses.
    /// </summary>
    [HttpGet("historico")]
    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "meses" })]
    public async Task<IActionResult> GetHistorico([FromQuery] int meses = 6)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Token inválido o usuario no identificado.")));
        }

        // Validación de rango
        if (meses < 1 || meses > 12)
        {
            return BadRequest(Result.Failure(Error.Validation("El número de meses debe estar entre 1 y 12.")));
        }

        // Reutilizamos la query principal (o podrías crear una específica GetDashboardHistoryQuery para ser más eficiente)
        var query = new GetDashboardResumenQuery(usuarioId.Value);
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        // 3. Filtrado y transformación de la respuesta
        // Envolvemos la lista en un Result.Success para mantener la consistencia del JSON
        var historico = result.Value.HistoricoUltimos6Meses.Take(meses).ToList();

        return HandleResult(Result.Success(historico));
    }
}