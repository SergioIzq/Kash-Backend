using Kash.Application.Features.Cuentas.Commands;
using Kash.Application.Features.Cuentas.Queries;
using Kash.Application.Features.Cuentas.Queries.Recent;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

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
    public async Task<IActionResult> GetPagedList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] string sortColumn = "",
        [FromQuery] string sortOrder = "")
    {
        // OPTIMIZACIÓN: Usamos el helper de la clase base
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetCuentasPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
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

        var query = new SearchCuentasQuery(search, limit)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Obtiene las cuentas más recientes del usuario.
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetRecentCuentasQuery(limit)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetCuentaByIdQuery(id);
        return await SendAndHandleAsync(query);
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

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCuentaCommand(id);
        return await SendAndHandleAsync(command);
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
