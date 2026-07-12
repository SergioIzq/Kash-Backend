using Kash.Application.Features.Conceptos.Commands;
using Kash.Application.Features.Conceptos.Queries;
using Kash.Application.Features.Conceptos.Queries.Recent;
using Kash.Application.Features.Conceptos.Queries.Search;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/conceptos")]
public class ConceptosController : AbsController
{
    public ConceptosController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de conceptos del usuario autenticado.
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

        var query = new GetConceptosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Búsqueda rápida para autocomplete.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search, [FromQuery] int limit = 10, [FromQuery] string? categoriaId = null)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new SearchConceptosQuery(search, limit)
        {
            UsuarioId = usuarioId,
            CategoriaId = categoriaId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Obtiene los conceptos más recientes.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5, [FromQuery] string? categoriaId = null)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetRecentConceptosQuery(limit, categoriaId)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetConceptoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConceptoRequest request)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized)
        {
            return unauthorized;
        }

        var command = new CreateConceptoCommand
        {
            Nombre = request.Nombre,
            CategoriaId = request.CategoriaId,
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConceptoRequest request)
    {
        var command = new UpdateConceptoCommand
        {
            Id = id,
            Nombre = request.Nombre,
            CategoriaId = request.CategoriaId
        };

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteConceptoCommand(id);
        return await SendAndHandleAsync(command);
    }
}

// DTOs
public record CreateConceptoRequest(
    string Nombre,
    Guid CategoriaId,
    Guid UsuarioId
);

public record UpdateConceptoRequest(
    string Nombre,
    Guid CategoriaId
);
