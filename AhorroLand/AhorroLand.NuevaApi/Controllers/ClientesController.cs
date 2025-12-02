using AhorroLand.Application.Features.Clientes.Commands;
using AhorroLand.Application.Features.Clientes.Queries;
using AhorroLand.Application.Features.Clientes.Queries.Recent;
using AhorroLand.Application.Features.Clientes.Queries.Search;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

[Authorize]
[ApiController]
[Route("api/clientes")]
public class ClientesController : AbsController
{
    public ClientesController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de clientes del usuario autenticado.
    /// Cacheada por 30s.
    /// </summary>
    [HttpGet]
    [OutputCache(Duration = 30, VaryByQueryKeys = new[] { "page", "pageSize", "searchTerm" })] // Agregué searchTerm por si acaso
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // ✅ OPTIMIZACIÓN: Usamos el helper de la clase base
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            // Retornamos un 401 usando el formato estandarizado
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetClientesPagedListQuery(page, pageSize)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Búsqueda rápida para autocomplete (selectores asíncronos).
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search, [FromQuery] int limit = 10)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new SearchClientesQuery(search, limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene los clientes más recientes del usuario.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetRecentClientesQuery(limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetClienteByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClienteRequest request)
    {
        // En el Create, a veces el UsuarioId viene en el request (si es admin creando para otro)
        // o lo tomamos del token si es auto-creación.
        // Aquí asumimos que si no viene, usamos el del token.
        var usuarioId = request.UsuarioId != Guid.Empty ? request.UsuarioId : GetCurrentUserId() ?? Guid.Empty;

        var command = new CreateClienteCommand(
            request.Nombre,
            usuarioId
        );

        var result = await _sender.Send(command);

        // Usamos HandleResultForCreation para devolver 201 Created y Location header
        return HandleResultForCreation(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty }
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClienteRequest request)
    {
        var command = new UpdateClienteCommand
        {
            Id = id,
            Nombre = request.Nombre
        };

        var result = await _sender.Send(command);
        return HandleResult(result); // Retorna 200 con el dato actualizado
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteClienteCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result); // Retorna 204 No Content si es éxito
    }
}

// DTOs de Request
public record CreateClienteRequest(string Nombre, Guid UsuarioId);
public record UpdateClienteRequest(string Nombre);