using Kash.Application.Features.Gastos.Commands;
using Kash.Application.Features.Gastos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/gastos")]
public class GastosController : AbsController
{
    public GastosController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene una lista paginada de gastos con soporte para búsqueda y ordenamiento.
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

        var query = new GetGastosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetGastoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGastoRequest request)
    {
        if (RequireCurrentUserId(out var userId) is { } unauthorized)
        {
            return unauthorized;
        }

        var command = new CreateGastoCommand
        {
            Importe = request.Importe,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            CategoriaNombre = request.CategoriaNombre,
            ConceptoNombre = request.ConceptoNombre,
            ConceptoId = request.ConceptoId,
            ProveedorId = request.ProveedorId,
            ProveedorNombre = request.ProveedorNombre,
            PersonaNombre = request.PersonaNombre,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            CuentaNombre = request.CuentaNombre,
            FormaPagoNombre = request.FormaPagoNombre,
            FormaPagoId = request.FormaPagoId,
            UsuarioId = userId
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGastoRequest request)
    {
        if (RequireCurrentUserId(out var userId) is { } unauthorized)
        {
            return unauthorized;
        }

        var command = new UpdateGastoCommand
        {
            Id = id,
            Importe = request.Importe,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            CategoriaNombre = request.CategoriaNombre,
            ConceptoNombre = request.ConceptoNombre,
            ConceptoId = request.ConceptoId,
            ProveedorId = request.ProveedorId,
            ProveedorNombre = request.ProveedorNombre,
            PersonaNombre = request.PersonaNombre,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            CuentaNombre = request.CuentaNombre,
            FormaPagoNombre = request.FormaPagoNombre,
            FormaPagoId = request.FormaPagoId,
            UsuarioId = userId
        };

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteGastoCommand(id);
        return await SendAndHandleAsync(command);
    }
}

// DTOs
public record CreateGastoRequest(
    decimal Importe,
    DateTime Fecha,
    string? Descripcion,
    Guid CategoriaId,
    Guid ConceptoId,
    Guid? ProveedorId,
    Guid? PersonaId,
    Guid CuentaId,
    Guid FormaPagoId,
    Guid UsuarioId, // CORREGIDO: Faltaba coma
                    // NUEVO: Nombres opcionales para auto-creación de entidades
    string? ConceptoNombre = null,
    string? CategoriaNombre = null,
    string? ProveedorNombre = null,
    string? PersonaNombre = null,
    string? FormaPagoNombre = null,
    string? CuentaNombre = null
);

public record UpdateGastoRequest(
    decimal Importe,
    DateTime Fecha,
    string? Descripcion,
    Guid CategoriaId,
    Guid ConceptoId,
    Guid? ProveedorId,
    Guid? PersonaId,
    Guid CuentaId,
    Guid FormaPagoId,
    string? ConceptoNombre = null,
    string? CategoriaNombre = null,
    string? ProveedorNombre = null,
    string? PersonaNombre = null,
    string? FormaPagoNombre = null,
    string? CuentaNombre = null
);
