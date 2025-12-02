using AhorroLand.Application.Features.Cuentas.Commands;
using AhorroLand.Application.Features.Cuentas.Queries;
using AhorroLand.Application.Features.Cuentas.Queries.Recent;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

[Authorize]
[ApiController]
[Route("api/cuentas")]
public class CuentasController : AbsController
{
    public CuentasController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de cuentas del usuario autenticado.
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
        var query = new GetCuentasPagedListQuery(page, pageSize)
        {
            UsuarioId = usuarioId.Value // 👈 IMPORTANTE: Asignar el ID para filtrar
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

        var query = new SearchCuentasQuery(search, limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene las cuentas más recientes del usuario.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetRecentCuentasQuery(limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCuentaByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCuentaRequest request)
    {
        // Asignación inteligente de UsuarioId (Token o Request)
        var usuarioId = request.UsuarioId != Guid.Empty ? request.UsuarioId : GetCurrentUserId() ?? Guid.Empty;

        var command = new CreateCuentaCommand
        {
            Nombre = request.Nombre,
            Saldo = request.Saldo,
            UsuarioId = usuarioId
        };

        var result = await _sender.Send(command);

        // Uso seguro de HandleResultForCreation (evita crash si result.Value falla)
        return HandleResultForCreation(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty }
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCuentaRequest request)
    {
        var command = new UpdateCuentaCommand
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
        var command = new DeleteCuentaCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

// DTOs
public record CreateCuentaRequest(
    string Nombre,
    decimal Saldo,
    Guid UsuarioId
);

public record UpdateCuentaRequest(
    string Nombre
);