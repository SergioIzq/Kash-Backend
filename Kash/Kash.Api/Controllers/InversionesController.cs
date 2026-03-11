using Kash.Application.Features.Inversiones.Commands;
using Kash.Application.Features.Inversiones.Commands.Import;
using Kash.Application.Features.Inversiones.Queries;
using Kash.NuevaApi.Controllers.Base;
using Kash.Shared.Domain.Abstractions.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.NuevaApi.Controllers;

[Authorize]
[ApiController]
[Route("api/inversiones")]
public class InversionesController : AbsController
{
    public InversionesController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// GET /api/inversiones?page=1&pageSize=100&searchTerm=AAPL
    /// Devuelve las inversiones del usuario autenticado, ordenadas por FechaCompra DESC.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPagedList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado.")));

        var query = new GetInversionesQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// GET /api/inversiones/{id}
    /// Devuelve una inversión por ID, validando que pertenezca al usuario autenticado.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetInversionByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// POST /api/inversiones
    /// Crea una nueva inversión para el usuario autenticado.
    /// Devuelve 201 Created con Location header.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInversionRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado.")));

        var command = new CreateInversionCommand
        {
            Nombre       = request.Nombre,
            Ticker       = request.Ticker,
            Tipo         = request.Tipo,
            Cantidad     = request.Cantidad,
            PrecioCompra = request.PrecioCompra,
            Moneda       = request.Moneda,
            FechaCompra  = request.FechaCompra,
            Descripcion  = request.Descripcion,
            Plataforma   = request.Plataforma
        };

        var result = await _sender.Send(command);

        return HandleResultForCreation(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// PUT /api/inversiones/{id}
    /// Actualización completa de una inversión. Valida propiedad del usuario.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInversionRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado.")));

        var command = new UpdateInversionCommand
        {
            Id           = id,
            Nombre       = request.Nombre,
            Ticker       = request.Ticker,
            Tipo         = request.Tipo,
            Cantidad     = request.Cantidad,
            PrecioCompra = request.PrecioCompra,
            Moneda       = request.Moneda,
            FechaCompra  = request.FechaCompra,
            Descripcion  = request.Descripcion,
            Plataforma   = request.Plataforma
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// DELETE /api/inversiones/{id}
    /// Elimina la inversión si pertenece al usuario autenticado.
    /// Devuelve 204 No Content.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteInversionCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// POST /api/inversiones/importar
    /// Importa inversiones desde un extracto CSV o PDF.
    /// Formatos: generic | trade_republic | trade_republic_pdf | degiro | interactive_brokers | binance
    /// </summary>
    [HttpPost("importar")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ImportarExtracto(
        [FromForm] string brokerFormat,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Archivo vacío.");

        using var stream = file.OpenReadStream();
        var bytes = new byte[file.Length];
        _ = await stream.ReadAsync(bytes);

        var command = new ImportarInversionesCommand(bytes, brokerFormat);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

// ──────────────────────────────────────────────────────────
// Request DTOs (entrada del frontend)
// ──────────────────────────────────────────────────────────

/// <summary>
/// Body para POST /api/inversiones.
/// El campo "tipo" acepta: "etf" | "accion" | "cripto" | "bono" | "fondo" | "mercado_privado" | "otro"
/// </summary>
public record CreateInversionRequest(
    string   Nombre,
    string   Ticker,
    string   Tipo,
    decimal  Cantidad,
    decimal  PrecioCompra,
    string   Moneda,
    DateTime FechaCompra,
    string?  Descripcion,
    string?  Plataforma);

/// <summary>
/// Body para PUT /api/inversiones/{id}.
/// Mismos campos que CreateInversionRequest (actualización completa).
/// </summary>
public record UpdateInversionRequest(
    string   Nombre,
    string   Ticker,
    string   Tipo,
    decimal  Cantidad,
    decimal  PrecioCompra,
    string   Moneda,
    DateTime FechaCompra,
    string?  Descripcion,
    string?  Plataforma);
