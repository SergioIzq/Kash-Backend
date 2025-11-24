using AhorroLand.Application.Features.Ingresos.Commands;
using AhorroLand.Application.Features.Ingresos.Queries;
using AhorroLand.NuevaApi.Controllers.Base;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AhorroLand.NuevaApi.Controllers;

[Authorize]
[ApiController]
[Route("api/ingresos")]
public class IngresosController : AbsController
{
    public IngresosController(ISender sender) : base(sender)
    {
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetIngresosPagedListQuery(page, pageSize);
        var result = await _sender.Send(query);
        return HandlePagedResult(result); // 🆕
    }

    /// <summary>
    /// Obtiene una lista paginada de ingresos con soporte para búsqueda y ordenamiento.
    /// </summary>
    /// <param name="page">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto: 10)</param>
    /// <param name="searchTerm">Término de búsqueda opcional (busca en: descripción, concepto, categoría, proveedor, persona, cuenta)</param>
    /// <param name="sortColumn">Columna por la cual ordenar (Fecha, Importe, ConceptoNombre, CategoriaNombre, ProveedorNombre, PersonaNombre, CuentaNombre, FormaPagoNombre)</param>
    /// <param name="sortOrder">Orden: 'asc' o 'desc' (por defecto: 'desc')</param>
    [Authorize]
    [HttpGet("paginated")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var usuarioId))
        {
            return Unauthorized(new { message = "Usuario no autenticado o token inválido" });
        }

        var query = new GetIngresosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId
        };

        var result = await _sender.Send(query);
        return HandlePagedResult(result); // 🆕
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetIngresoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngresoRequest request)
    {
        var command = new CreateIngresoCommand
        {
            Importe = request.Importe,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            CategoriaNombre = request.CategoriaNombre,
            ConceptoId = request.ConceptoId,
            ConceptoNombre = request.ConceptoNombre,
            ClienteId = request.ClienteId,
            ClienteNombre = request.ClienteNombre,
            PersonaId = request.PersonaId,
            PersonaNombre = request.PersonaNombre,
            CuentaId = request.CuentaId,
            CuentaNombre = request.CuentaNombre,
            FormaPagoId = request.FormaPagoId,
            FormaPagoNombre = request.FormaPagoNombre,
            UsuarioId = request.UsuarioId
        };

        var result = await _sender.Send(command);

        return HandleResultForCreation(
            result,
            nameof(GetById),
        new { id = result.Value.Id }
        );
    }

    [Authorize]
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
            UsuarioId = request.UsuarioId
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteIngresoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

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
