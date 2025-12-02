using AhorroLand.Application.Features.Ingresos.Commands;
using AhorroLand.Application.Features.Ingresos.Queries;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

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
    [HttpGet] // Estandarizado a la raíz (GET /api/ingresos)
    [OutputCache(Duration = 30, VaryByQueryKeys = new[] { "page", "pageSize", "searchTerm", "sortColumn", "sortOrder" })]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null)
    {
        // 1. Obtener ID del usuario de forma segura
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetIngresosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId.Value // 👈 Asignación crítica para seguridad
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
        // Asignación inteligente de UsuarioId
        var usuarioId = request.UsuarioId != Guid.Empty ? request.UsuarioId : GetCurrentUserId() ?? Guid.Empty;

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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIngresoRequest request)
    {
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
            UsuarioId = request.UsuarioId // Nota: Validar si permites cambiar de dueño en el handler
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
    string CategoriaNombre,
    Guid ConceptoId,
    string ConceptoNombre,
    Guid ClienteId,
    string ClienteNombre,
    Guid PersonaId,
    string PersonaNombre,
    Guid CuentaId,
    string CuentaNombre,
    Guid FormaPagoId,
    string FormaPagoNombre,
    Guid UsuarioId
);

public record UpdateIngresoRequest(
    decimal Importe,
    DateTime Fecha,
    string? Descripcion,
    Guid CategoriaId,
    Guid ConceptoId,
    Guid ClienteId,
    Guid PersonaId,
    Guid CuentaId,
    Guid FormaPagoId,
    Guid UsuarioId
);