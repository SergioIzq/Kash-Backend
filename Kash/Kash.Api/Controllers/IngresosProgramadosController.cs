using Kash.Application.Features.IngresosProgramados.Commands;
using Kash.Application.Features.IngresosProgramados.Queries;
using SergioIzq.AspNetCore.Kernel.Controllers;
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/ingresos-programados")]
public class IngresosProgramadosController : AbsController
{
    public IngresosProgramadosController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Obtiene lista paginada de ingresos programados del usuario autenticado.
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

        var query = new GetIngresosProgramadosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetIngresoProgramadoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngresoProgramadoRequest request)
    {
        // 1. Obtener ID del usuario
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        // 2. Crear comando con UsuarioId inyectado
        var command = new CreateIngresoProgramadoCommand
        {
            Importe = request.Importe,
            Frecuencia = request.Frecuencia,
            FechaEjecucion = request.FechaEjecucion,
            Descripcion = request.Descripcion,
            ConceptoId = request.ConceptoId,
            ConceptoNombre = request.ConceptoNombre,
            CategoriaId = request.CategoriaId,
            ClienteId = request.ClienteId,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            FormaPagoId = request.FormaPagoId,
            Activo = request.Activo,
            CategoriaNombre = request.CategoriaNombre,
            CuentaNombre = request.CuentaNombre,
            FormaPagoNombre = request.FormaPagoNombre,
            UsuarioId = usuarioId
        };

        var result = await _sender.Send(command);

        // 3. Respuesta segura
        return HandleResultForCreation(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value : Guid.Empty }
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIngresoProgramadoRequest request)
    {
        var command = new UpdateIngresoProgramadoCommand
        {
            Id = id,
            Importe = request.Importe,
            Frecuencia = request.Frecuencia,
            FechaEjecucion = request.FechaEjecucion,
            Descripcion = request.Descripcion,
            ConceptoId = request.ConceptoId,
            ConceptoNombre = request.ConceptoNombre,
            CategoriaId = request.CategoriaId,
            ClienteId = request.ClienteId,
            ClienteNombre = request.ClienteNombre,
            PersonaId = request.PersonaId,
            PersonaNombre = request.PersonaNombre,
            CuentaId = request.CuentaId,
            CuentaNombre = request.CuentaNombre,
            FormaPagoId = request.FormaPagoId,
            FormaPagoNombre = request.FormaPagoNombre,
            Activo = request.Activo,
            CategoriaNombre = request.CategoriaNombre,
            UsuarioId = request.UsuarioId
        };

        return await SendAndHandleAsync(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteIngresoProgramadoCommand(id);
        return await SendAndHandleAsync(command);
    }
}

// DTOs
public record CreateIngresoProgramadoRequest(
    decimal Importe,
    string Frecuencia,
    DateTime FechaEjecucion,
    string? Descripcion,
    Guid ConceptoId,
    string ConceptoNombre,
    Guid CategoriaId,
    string CategoriaNombre,
    Guid? ClienteId,
    string? ClienteNombre,
    Guid? PersonaId,
    string? PersonaNombre,
    Guid CuentaId,
    string? CuentaNombre,
    Guid FormaPagoId,
    string? FormaPagoNombre,
    bool Activo
);

public record UpdateIngresoProgramadoRequest(
    decimal Importe,
    string Frecuencia,
    DateTime FechaEjecucion,
    string? Descripcion,
    Guid ConceptoId,
    string ConceptoNombre,
    Guid CategoriaId,
    string CategoriaNombre,
    Guid? ClienteId,
    string? ClienteNombre,
    Guid? PersonaId,
    string? PersonaNombre,
    Guid CuentaId,
    string? CuentaNombre,
    Guid FormaPagoId,
    string? FormaPagoNombre,
    bool Activo,
    Guid UsuarioId
);
