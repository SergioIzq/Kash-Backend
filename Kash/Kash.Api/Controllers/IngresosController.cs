using Kash.Application.Features.Ingresos.Commands;
using Kash.Application.Features.Ingresos.Queries;
using Kash.NuevaApi.Controllers.Base;
using Kash.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.NuevaApi.Controllers;

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
        // ✅ OPTIMIZACIÓN: Usamos el helper de la clase base
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            // Retornamos un 401 usando el formato estandarizado
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetIngresosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetIngresoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngresoRequest request)
    {
        var userId = GetCurrentUserId();

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
            UsuarioId = userId!.Value,
            // 🔥 NUEVO: Pasar nombres para auto-creación
            ConceptoNombre = request.ConceptoNombre,
            ClienteNombre = request.ClienteNombre,
            PersonaNombre = request.PersonaNombre
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
        var userId = GetCurrentUserId();

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
            UsuarioId = userId!.Value
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteIngresoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
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
    Guid UsuarioId, // 🔥 CORREGIDO: Faltaba coma
    // 🔥 NUEVO: Nombres opcionales para auto-creación de entidades
    string? ConceptoNombre = null,
    string? ClienteNombre = null,
    string? PersonaNombre = null
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
    Guid FormaPagoId
);