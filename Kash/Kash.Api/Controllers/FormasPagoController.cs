using Kash.Application.Features.FormasPago.Commands;
using Kash.Application.Features.FormasPago.Queries;
using Kash.Application.Features.FormasPago.Queries.Recent;
using Kash.Application.Features.FormasPago.Queries.Search; // Asegúrate de tener este namespace
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

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
    public async Task<IActionResult> GetPagedList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] string sortColumn = "",
        [FromQuery] string sortOrder = "")
    {
        // OPTIMIZACIÓN: Usamos el helper de la clase base
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetFormasPagoPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
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

        var query = new SearchFormasPagoQuery(search, limit)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Obtiene las formas de pago más recientes.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetRecentFormasPagoQuery(limit)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetFormaPagoByIdQuery(id);
        return await SendAndHandleAsync(query);
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

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteFormaPagoCommand(id);
        return await SendAndHandleAsync(command);
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
