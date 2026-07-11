using Kash.Application.Features.Clientes.Commands;
using Kash.Application.Features.Clientes.Queries;
using Kash.Application.Features.Clientes.Queries.Recent;
using Kash.Application.Features.Clientes.Queries.Search;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

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
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string searchTerm = "", [FromQuery] string sortColumn = "", [FromQuery] string sortOrder = "")
    {
        // OPTIMIZACIÓN: Usamos el helper de la clase base
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetClientesPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Búsqueda rápida para autocomplete (selectores asíncronos).
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search, [FromQuery] int limit = 10)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new SearchClientesQuery(search, limit)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Obtiene los clientes más recientes del usuario.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetRecentClientesQuery(limit)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetClienteByIdQuery(id);
        return await SendAndHandleAsync(query);
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

        return await SendAndHandleAsync(command); // Retorna 200 con el dato actualizado
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteClienteCommand(id);
        return await SendAndHandleAsync(command); // Retorna 204 No Content si es éxito
    }
}

// DTOs de Request
public record CreateClienteRequest(string Nombre, Guid UsuarioId);
public record UpdateClienteRequest(string Nombre);
