using AhorroLand.Application.Features.TraspasosProgramados.Commands;
using AhorroLand.Application.Features.TraspasosProgramados.Queries;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

[Authorize]
[ApiController]
[Route("api/traspasos-programados")]
public class TraspasosProgramadosController : AbsController
{
    public TraspasosProgramadosController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de traspasos programados del usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // 1. Obtener ID del usuario (Seguridad)
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        // 2. Crear query filtrando por usuario
        var query = new GetTraspasosProgramadosPagedListQuery(page, pageSize)
        {
            UsuarioId = usuarioId.Value // 👈 IMPORTANTE: Asignar el ID
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetTraspasoProgramadoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTraspasoProgramadoRequest request)
    {
        // Asignación inteligente de UsuarioId
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var command = new CreateTraspasoProgramadoCommand
        {
            CuentaOrigenId = request.CuentaOrigenId,
            CuentaDestinoId = request.CuentaDestinoId,
            Importe = request.Importe,
            FechaEjecucion = request.FechaEjecucion,
            Frecuencia = request.Frecuencia,
            UsuarioId = usuarioId.Value, // 👈 Seguridad: ID del token
            Descripcion = request.Descripcion
        };

        var result = await _sender.Send(command);

        // Uso seguro de HandleResultForCreation
        return HandleResultForCreation(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty }
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTraspasoProgramadoRequest request)
    {
        var command = new UpdateTraspasoProgramadoCommand
        {
            Id = id,
            CuentaOrigenId = request.CuentaOrigenId,
            CuentaDestinoId = request.CuentaDestinoId,
            Importe = request.Importe,
            FechaEjecucion = request.FechaEjecucion,
            Frecuencia = request.Frecuencia,
            // Nota: El UsuarioId generalmente no cambia en un update, 
            // pero si tu comando lo requiere para validación de propiedad:
            UsuarioId = GetCurrentUserId() ?? Guid.Empty,
            HangfireJobId = request.HangfireJobId,
            Descripcion = request.Descripcion
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteTraspasoProgramadoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

// DTOs simplificados (sin UsuarioId, ya que se inyecta en el Controller)
public record CreateTraspasoProgramadoRequest(
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Importe,
    DateTime FechaEjecucion,
    string Frecuencia,
    string? Descripcion
);

public record UpdateTraspasoProgramadoRequest(
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Importe,
    DateTime FechaEjecucion,
    string Frecuencia,
    string HangfireJobId,
    string? Descripcion
);