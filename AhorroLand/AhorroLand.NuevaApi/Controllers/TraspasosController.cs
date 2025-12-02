using AhorroLand.Application.Features.Traspasos.Commands;
using AhorroLand.Application.Features.Traspasos.Queries;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

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
    [OutputCache(Duration = 30, VaryByQueryKeys = new[] { "page", "pageSize" })]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // 1. Obtener ID del usuario (Seguridad)
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        // 2. Crear query filtrando por usuario
        var query = new GetTraspasosPagedListQuery(page, pageSize)
        {
            UsuarioId = usuarioId.Value // 👈 IMPORTANTE: Asignar el ID
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetTraspasoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
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
            UsuarioId = usuarioId, // 👈 Seguridad: Usar ID validado
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
            Fecha = request.Fecha,
            Descripcion = request.Descripcion
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteTraspasoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
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