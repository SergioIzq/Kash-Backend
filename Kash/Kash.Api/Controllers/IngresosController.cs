using Kash.Application.Features.Ingresos.Commands;
using Kash.Application.Features.Ingresos.Queries;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/ingresos")]
public class IngresosController : AbsController
{
    public IngresosController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene una lista paginada de ingresos con soporte para búsqueda y ordenamiento.
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

        var query = new GetIngresosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetIngresoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngresoRequest request)
    {
        if (RequireCurrentUserId(out var userId) is { } unauthorized)
        {
            return unauthorized;
        }

        var command = new CreateIngresoCommand
        {
            Importe = request.Importe,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            ConceptoId = request.ConceptoId,
            ClienteId = request.ClienteId,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            FormaPagoId = request.FormaPagoId,
            UsuarioId = userId,
            // NUEVO: Pasar nombres para auto-creación
            ConceptoNombre = request.ConceptoNombre,
            CategoriaNombre = request.CategoriaNombre,
            CuentaNombre = request.CuentaNombre,
            ClienteNombre = request.ClienteNombre,
            PersonaNombre = request.PersonaNombre,
            FormaPagoNombre = request.FormaPagoNombre
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIngresoRequest request)
    {
        if (RequireCurrentUserId(out var userId) is { } unauthorized)
        {
            return unauthorized;
        }

        var command = new UpdateIngresoCommand
        {
            Id = id,
            Importe = request.Importe,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            ConceptoId = request.ConceptoId,
            ClienteId = request.ClienteId,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            FormaPagoId = request.FormaPagoId,
            UsuarioId = userId,
            // NUEVO: Pasar nombres para auto-creación
            ConceptoNombre = request.ConceptoNombre,
            CategoriaNombre = request.CategoriaNombre,
            ClienteNombre = request.ClienteNombre,
            PersonaNombre = request.PersonaNombre,
            FormaPagoNombre = request.FormaPagoNombre,
            CuentaNombre = request.CuentaNombre
        };

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteIngresoCommand(id);
        return await SendAndHandleAsync(command);
    }
}

// DTOs
public record CreateIngresoRequest(
    decimal Importe,
    DateTime Fecha,
    string? Descripcion,
    Guid CategoriaId,
    Guid ConceptoId,
    Guid? ClienteId,
    Guid? PersonaId,
    Guid CuentaId,
    Guid FormaPagoId,
    Guid UsuarioId, // CORREGIDO: Faltaba coma
    // NUEVO: Nombres opcionales para auto-creación de entidades
    string? ConceptoNombre = null,
    string? CategoriaNombre = null,
    string? ClienteNombre = null,
    string? PersonaNombre = null,
    string? FormaPagoNombre = null,
    string? CuentaNombre = null
);

public record UpdateIngresoRequest(
    decimal Importe,
    DateTime Fecha,
    string? Descripcion,
    Guid CategoriaId,
    Guid ConceptoId,
    Guid? ClienteId,
    Guid? PersonaId,
    Guid CuentaId,
    Guid FormaPagoId,
    string? ConceptoNombre = null,
    string? CategoriaNombre = null,
    string? ClienteNombre = null,
    string? PersonaNombre = null,
    string? FormaPagoNombre = null,
    string? CuentaNombre = null
);
