using AhorroLand.Application.Features.Proveedores.Commands;
using AhorroLand.Application.Features.Proveedores.Queries;
using AhorroLand.Application.Features.Proveedores.Queries.Recent;
using AhorroLand.Application.Features.Proveedores.Queries.Search; // Asegúrate de tener este namespace
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

[Authorize]
[ApiController]
[Route("api/proveedores")]
public class ProveedoresController : AbsController
{
    public ProveedoresController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de proveedores del usuario autenticado.
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
        var query = new GetProveedoresPagedListQuery(page, pageSize)
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

        var query = new SearchProveedoresQuery(search, limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene los proveedores más recientes.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetRecentProveedoresQuery(limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProveedorByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProveedorRequest request)
    {
        // Asignación inteligente de UsuarioId
        var usuarioId = request.UsuarioId != Guid.Empty ? request.UsuarioId : GetCurrentUserId() ?? Guid.Empty;

        var command = new CreateProveedorCommand
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProveedorRequest request)
    {
        var command = new UpdateProveedorCommand
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
        var command = new DeleteProveedorCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

// DTOs
public record CreateProveedorRequest(
    string Nombre,
    Guid UsuarioId
);

public record UpdateProveedorRequest(
    string Nombre
);