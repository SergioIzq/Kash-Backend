using Kash.Application.Features.ReglasCategorizacion.Commands;
using Kash.Application.Features.ReglasCategorizacion.Queries;
using Kash.Api.Controllers.Base;
using Kash.Shared.Domain.Abstractions.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

/// <summary>
/// CRUD de reglas de auto-categorización, usadas al previsualizar/importar extractos bancarios
/// para proponer automáticamente la categoría/concepto/proveedor de cada movimiento.
/// </summary>
[Authorize]
[ApiController]
[Route("api/reglas-categorizacion")]
public class ReglasCategorizacionController : AbsController
{
    public ReglasCategorizacionController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de reglas del usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPagedList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] string sortColumn = "",
        [FromQuery] string sortOrder = "")
    {
        var usuarioId = GetCurrentUserId();
        if (usuarioId is null)
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));

        var query = new GetReglasCategorizacionPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetReglaCategorizacionByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReglaCategorizacionRequest request)
    {
        var usuarioId = GetCurrentUserId();
        if (usuarioId is null)
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));

        var command = new CreateReglaCategorizacionCommand
        {
            Patron = request.Patron,
            Tipo = request.Tipo,
            CategoriaNombre = request.CategoriaNombre,
            ConceptoNombre = request.ConceptoNombre,
            ProveedorNombre = request.ProveedorNombre,
            FormaPagoNombre = request.FormaPagoNombre,
            Prioridad = request.Prioridad,
            Activo = request.Activo,
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(command);

        return HandleResultForCreation(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReglaCategorizacionRequest request)
    {
        var command = new UpdateReglaCategorizacionCommand
        {
            Id = id,
            Patron = request.Patron,
            Tipo = request.Tipo,
            CategoriaNombre = request.CategoriaNombre,
            ConceptoNombre = request.ConceptoNombre,
            ProveedorNombre = request.ProveedorNombre,
            FormaPagoNombre = request.FormaPagoNombre,
            Prioridad = request.Prioridad,
            Activo = request.Activo
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteReglaCategorizacionCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

public record CreateReglaCategorizacionRequest(
    string Patron,
    string? Tipo,
    string CategoriaNombre,
    string? ConceptoNombre,
    string? ProveedorNombre,
    string? FormaPagoNombre,
    int Prioridad,
    bool Activo);

public record UpdateReglaCategorizacionRequest(
    string Patron,
    string? Tipo,
    string CategoriaNombre,
    string? ConceptoNombre,
    string? ProveedorNombre,
    string? FormaPagoNombre,
    int Prioridad,
    bool Activo);
