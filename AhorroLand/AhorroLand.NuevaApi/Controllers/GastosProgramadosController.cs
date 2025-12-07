using AhorroLand.Application.Features.GastosProgramados.Commands;
using AhorroLand.Application.Features.GastosProgramados.Queries;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

[Authorize]
[ApiController]
[Route("api/gastos-programados")]
public class GastosProgramadosController : AbsController
{
    public GastosProgramadosController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // 1. Seguridad: Obtener ID del usuario
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        // 2. Query filtrada por usuario
        var query = new GetGastosProgramadosPagedListQuery(page, pageSize)
        {
            UsuarioId = usuarioId.Value // 👈 Asignación crítica
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetGastoProgramadoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGastoProgramadoRequest request)
    {
        // 1. Obtener ID del usuario
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

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
            CategoriaId = request.CategoriaId,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            FormaPagoId = request.FormaPagoId,
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
            CategoriaId = request.CategoriaId,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            FormaPagoId = request.FormaPagoId
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteGastoProgramadoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

// DTOs
public record CreateGastoProgramadoRequest(
    decimal Importe,
    string Frecuencia,
    DateTime? FechaEjecucion,
    string? Descripcion,
    Guid ConceptoId,
    string ConceptoNombre,
    Guid ProveedorId,
    Guid CategoriaId,
    Guid PersonaId,
    Guid CuentaId,
    Guid FormaPagoId
);

public record UpdateGastoProgramadoRequest(
    decimal Importe,
    string Frecuencia,
    DateTime? FechaEjecucion,
    string? Descripcion,
    Guid ConceptoId,
    string ConceptoNombre,
    Guid ProveedorId,
    Guid CategoriaId,
    Guid PersonaId,
    Guid CuentaId,
    Guid FormaPagoId
);