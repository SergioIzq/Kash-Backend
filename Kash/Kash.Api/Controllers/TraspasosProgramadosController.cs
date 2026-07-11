using Kash.Application.Features.TraspasosProgramados.Commands;
using Kash.Application.Features.TraspasosProgramados.Queries;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

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
    public async Task<IActionResult> GetPagedList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] string sortColumn = "",
        [FromQuery] string sortOrder = "")
    {
        // OPTIMIZACIÓN: Usamos el helper de la clase base
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetTraspasosProgramadosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetTraspasoProgramadoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTraspasoProgramadoRequest request)
    {
        // Asignación inteligente de UsuarioId
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var command = new CreateTraspasoProgramadoCommand
        {
            CuentaOrigenId = request.CuentaOrigenId,
            CuentaDestinoId = request.CuentaDestinoId,
            Importe = request.Importe,
            FechaEjecucion = request.FechaEjecucion,
            Frecuencia = request.Frecuencia,
            UsuarioId = usuarioId, // Seguridad: ID del token
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

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteTraspasoProgramadoCommand(id);
        return await SendAndHandleAsync(command);
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
