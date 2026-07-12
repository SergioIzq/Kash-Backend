using Kash.Application.Features.Categorias.Commands;
using Kash.Application.Features.Categorias.Queries;
using Kash.Application.Features.Categorias.Queries.Recent;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/categorias")]
public class CategoriasController : AbsController
{
    public CategoriasController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de categorías del usuario autenticado.
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

        var query = new GetCategoriasPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Búsqueda rápida para autocomplete.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search, [FromQuery] int limit = 10)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new SearchCategoriasQuery(search, limit)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Obtiene las categorías más recientes del usuario.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetRecentCategoriasQuery(limit)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCategoriaByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaRequest request)
    {
        // Asignación inteligente del UsuarioId
        var usuarioId = request.UsuarioId != Guid.Empty ? request.UsuarioId : GetCurrentUserId() ?? Guid.Empty;

        var command = new CreateCategoriaCommand
        {
            Nombre = request.Nombre,
            UsuarioId = usuarioId,
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoriaRequest request)
    {
        var command = new UpdateCategoriaCommand
        {
            Id = id,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion
        };

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCategoriaCommand(id);
        return await SendAndHandleAsync(command);
    }
}

// DTOs de Request
public record CreateCategoriaRequest(
    string Nombre,
    Guid UsuarioId,
    string? Descripcion
);

public record UpdateCategoriaRequest(
    string Nombre,
    string? Descripcion
);
