using Kash.Application.Features.Traspasos.Commands;
using Kash.Application.Features.Traspasos.Queries;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/traspasos")]
public class TraspasosController : AbsController
{
    public TraspasosController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de traspasos del usuario autenticado.
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

        var query = new GetTraspasosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetTraspasoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTraspasoRequest request)
    {
        // Asignación inteligente de UsuarioId
        var usuarioId = request.UsuarioId != Guid.Empty ? request.UsuarioId : GetCurrentUserId() ?? Guid.Empty;

        var command = new CreateTraspasoCommand
        {
            CuentaOrigenId = request.CuentaOrigenId,
            CuentaDestinoId = request.CuentaDestinoId,
            UsuarioId = usuarioId, // Seguridad: Usar ID validado
            Importe = request.Importe,
            Fecha = request.Fecha,
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTraspasoRequest request)
    {
        var command = new UpdateTraspasoCommand
        {
            Id = id,
            CuentaOrigenId = request.CuentaOrigenId,
            CuentaDestinoId = request.CuentaDestinoId,
            Importe = request.Importe,
            FechaEjecucion = request.Fecha,
            Descripcion = request.Descripcion
        };

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteTraspasoCommand(id);
        return await SendAndHandleAsync(command);
    }
}

// DTOs
public record CreateTraspasoRequest(
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    Guid UsuarioId,
    decimal Importe,
    DateTime Fecha,
    string? Descripcion
);

public record UpdateTraspasoRequest(
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Importe,
    DateTime Fecha,
    string? Descripcion
);
