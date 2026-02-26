using Kash.Application.Features.IngresosProgramados.Commands;
using Kash.Application.Features.IngresosProgramados.Queries;
using Kash.NuevaApi.Controllers.Base;
using Kash.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.NuevaApi.Controllers;

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
        // ✅ OPTIMIZACIÓN: Usamos el helper de la clase base
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            // Retornamos un 401 usando el formato estandarizado
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetIngresosProgramadosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetIngresoProgramadoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngresoProgramadoRequest request)
    {
        // 1. Obtener ID del usuario
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

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
            CategoriaNombre = request.CategoriaNombre
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
            CategoriaNombre = request.CategoriaNombre
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteIngresoProgramadoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
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
    bool Activo
);