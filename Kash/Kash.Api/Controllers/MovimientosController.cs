using Kash.Application.Features.Movimientos.Commands.Confirm;
using Kash.Application.Features.Movimientos.Commands.Import;
using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Application.Features.Movimientos.Commands.Preview;
using Kash.Api.Controllers.Base;
using Kash.Shared.Domain.Abstractions.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kash.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/movimientos")]
public class MovimientosController : AbsController
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MovimientosController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// POST /api/movimientos/previsualizar
    /// Parsea el extracto (CSV o PDF) y devuelve los movimientos que se CREARÍAN, marcando
    /// duplicados, para que el usuario los revise y edite. No crea nada.
    /// </summary>
    [HttpPost("previsualizar")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Previsualizar([FromForm] string mapping, IFormFile file)
    {
        var (map, error) = ParseMapping(mapping, file);
        if (error is not null) return error;

        var bytes = await LeerBytesAsync(file);
        var result = await _sender.Send(new PrevisualizarMovimientosCommand(bytes, map!));
        return HandleResult(result);
    }

    /// <summary>
    /// POST /api/movimientos/confirmar
    /// Crea los movimientos ya revisados y editados por el usuario, tal cual llegan.
    /// </summary>
    [HttpPost("confirmar")]
    public async Task<IActionResult> Confirmar([FromBody] ConfirmarMovimientosRequest request)
    {
        if (request?.Movimientos is null || request.Movimientos.Count == 0)
            return BadRequest(Result.Failure(Error.Validation("No hay movimientos que confirmar.")));

        var command = new ConfirmarMovimientosCommand(request.Movimientos);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// POST /api/movimientos/importar
    /// Importación directa (sin revisión) desde un extracto CSV o PDF de cualquier banco.
    /// </summary>
    [HttpPost("importar")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ImportarExtracto([FromForm] string mapping, IFormFile file)
    {
        var (map, error) = ParseMapping(mapping, file);
        if (error is not null) return error;

        var bytes = await LeerBytesAsync(file);
        var result = await _sender.Send(new ImportarMovimientosCommand(bytes, map!));
        return HandleResult(result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private (ColumnMapping? map, IActionResult? error) ParseMapping(string mapping, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return (null, BadRequest(Result.Failure(Error.Validation("Archivo vacío."))));

        ColumnMapping? map;
        try
        {
            map = JsonSerializer.Deserialize<ColumnMapping>(mapping, JsonOptions);
        }
        catch (JsonException ex)
        {
            return (null, BadRequest(Result.Failure(
                Error.Validation($"Configuración de columnas inválida: {ex.Message}"))));
        }

        if (map is null || string.IsNullOrWhiteSpace(map.CuentaNombre))
            return (null, BadRequest(Result.Failure(
                Error.Validation("Debes indicar la cuenta y el mapeo de columnas."))));

        return (map, null);
    }

    private static async Task<byte[]> LeerBytesAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var bytes = new byte[file.Length];
        _ = await stream.ReadAsync(bytes);
        return bytes;
    }
}

/// <summary>Body de POST /api/movimientos/confirmar.</summary>
public record ConfirmarMovimientosRequest(List<MovimientoConfirmarDto> Movimientos);
