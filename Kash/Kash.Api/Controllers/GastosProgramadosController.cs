using Kash.Application.Features.GastosProgramados.Commands;
using Kash.Application.Features.GastosProgramados.Queries;
using Kash.Domain;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error y Result
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/gastos-programados")]
public class GastosProgramadosController : AbsController
{
    public GastosProgramadosController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de gastos programados del usuario autenticado.
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

        var query = new GetGastosProgramadosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetGastoProgramadoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGastoProgramadoRequest request)
    {
        // 1. Obtener ID del usuario
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        // 2. Crear comando con el ID del usuario inyectado
        var command = new CreateGastoProgramadoCommand
        {
            Importe = request.Importe,
            Frecuencia = request.Frecuencia,
            FechaEjecucion = request.FechaEjecucion,
            Descripcion = request.Descripcion,
            ConceptoId = request.ConceptoId,
            ConceptoNombre = request.ConceptoNombre,
            ProveedorId = request.ProveedorId,
            ProveedorNombre = request.ProveedorNombre,
            CategoriaNombre = request.CategoriaNombre,
            PersonaNombre = request.PersonaNombre,
            FormaPagoNombre = request.FormaPagoNombre,
            CategoriaId = request.CategoriaId,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            FormaPagoId = request.FormaPagoId,
            UsuarioId = usuarioId
        };

        var result = await _sender.Send(command);

        // 3. Respuesta segura 201 Created
        return HandleResultForCreation(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty }
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGastoProgramadoRequest request)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var command = new UpdateGastoProgramadoCommand
        {
            Id = id,
            Importe = request.Importe,
            Frecuencia = request.Frecuencia,
            FechaEjecucion = request.FechaEjecucion,
            Descripcion = request.Descripcion,
            ConceptoId = request.ConceptoId,
            ConceptoNombre = request.ConceptoNombre,
            ProveedorId = request.ProveedorId,
            ProveedorNombre = request.ProveedorNombre,
            CategoriaNombre = request.CategoriaNombre,
            PersonaNombre = request.PersonaNombre,
            FormaPagoNombre = request.FormaPagoNombre,
            CategoriaId = request.CategoriaId,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            FormaPagoId = request.FormaPagoId,
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteGastoProgramadoCommand(id);
        return await SendAndHandleAsync(command);
    }
}

// DTOs
public record CreateGastoProgramadoRequest(
    decimal Importe,
    string Frecuencia,
    DateTime? FechaEjecucion,
    string? Descripcion,
    Guid ConceptoId,
    Guid? ProveedorId,
    Guid CategoriaId,
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

public record UpdateGastoProgramadoRequest(
    decimal Importe,
    string Frecuencia,
    DateTime? FechaEjecucion,
    string? Descripcion,
    Guid ConceptoId,
    Guid? ProveedorId,
    Guid CategoriaId,
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
