using AhorroLand.Application.Features.FormasPago.Commands;
using AhorroLand.Application.Features.FormasPago.Queries;
using AhorroLand.Application.Features.FormasPago.Queries.Recent;
using AhorroLand.Application.Features.FormasPago.Queries.Search; // Asegúrate de tener este namespace
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

[Authorize]
[ApiController]
[Route("api/formas-pago")]
public class FormasPagoController : AbsController
{
    public FormasPagoController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de formas de pago del usuario autenticado.
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
        var query = new GetFormasPagoPagedListQuery(page, pageSize)
        {
            UsuarioId = usuarioId.Value // 👈 IMPORTANTE: Asignar el ID
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Búsqueda rápida para autocomplete.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search, [FromQuery] int limit = 10)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new SearchFormasPagoQuery(search, limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene las formas de pago más recientes.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetRecentFormasPagoQuery(limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetFormaPagoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFormaPagoRequest request)
    {
        // Asignación inteligente de UsuarioId
        var usuarioId = request.UsuarioId != Guid.Empty ? request.UsuarioId : GetCurrentUserId() ?? Guid.Empty;

        var command = new CreateFormaPagoCommand
        {
            Nombre = request.Nombre,
            UsuarioId = usuarioId
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFormaPagoRequest request)
    {
        var command = new UpdateFormaPagoCommand
        {
            Id = id,
            Nombre = request.Nombre
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteFormaPagoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

// DTOs
public record CreateFormaPagoRequest(
    string Nombre,
    Guid UsuarioId
);

public record UpdateFormaPagoRequest(
    string Nombre
);