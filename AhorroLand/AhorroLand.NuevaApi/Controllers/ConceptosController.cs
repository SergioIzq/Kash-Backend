using AhorroLand.Application.Features.Conceptos.Commands;
using AhorroLand.Application.Features.Conceptos.Queries;
using AhorroLand.Application.Features.Conceptos.Queries.Recent;
using AhorroLand.Application.Features.Conceptos.Queries.Search;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

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
        var query = new GetConceptosPagedListQuery(page, pageSize)
        {
            UsuarioId = usuarioId.Value
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

        var query = new SearchConceptosQuery(search, limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene los conceptos más recientes.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetRecentConceptosQuery(limit)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetConceptoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConceptoRequest request)
    {
        // Asignación inteligente de UsuarioId (Token o Request)
        var usuarioId = GetCurrentUserId();

        var command = new CreateConceptoCommand
        {
            Nombre = request.Nombre,
            CategoriaId = request.CategoriaId,
            UsuarioId = usuarioId!.Value
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

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteConceptoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
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