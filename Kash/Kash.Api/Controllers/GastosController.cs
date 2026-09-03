using Kash.Application.Features.Gastos.Commands;
using Kash.Application.Features.Gastos.Queries;
using Kash.Application.Features.Gastos.Queries.GetExcel;
using Kash.Application.Features.Gastos.Queries.GetPeriodo;
using Kash.Application.Features.Gastos.Queries.Habituales;
using Kash.Application.Features.Gastos.Queries.Sugerencia;
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

    /// <summary>
    /// Obtiene los Gastos del usuario cuya fecha de transacción cae dentro de un rango indicado,
    /// paginados, para vistas de movimientos rápidos filtrables por periodo.
    /// </summary>
    [HttpGet("periodo")]
    public async Task<IActionResult> GetPorPeriodo(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetGastosPorPeriodoQuery(usuarioId, fechaInicio, fechaFin, page, pageSize);

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Genera y descarga un Excel con los Gastos del usuario que cumplen los filtros indicados
    /// (todos opcionales y combinables), sin paginar.
    /// </summary>
    [HttpGet("excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetExcel(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? searchTerm,
        [FromQuery] Guid[]? conceptoIds,
        [FromQuery] Guid[]? categoriaIds,
        [FromQuery] Guid[]? proveedorIds,
        [FromQuery] Guid[]? personaIds,
        CancellationToken cancellationToken)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetGastosExcelQuery(usuarioId, fechaInicio, fechaFin, searchTerm, conceptoIds, categoriaIds, proveedorIds, personaIds);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return File(result.Value.Contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Value.NombreArchivo);
        }

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetGastoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Obtiene la combinación (cuenta, forma de pago, importe, proveedor, persona) del
    /// gasto más reciente registrado por el usuario para un concepto dado, para pre-rellenar
    /// un alta nueva. Devuelve una lista vacía si el concepto no tiene gastos previos.
    /// </summary>
    [HttpGet("sugerencia")]
    public async Task<IActionResult> GetSugerencia([FromQuery] Guid conceptoId)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetSugerenciaGastoQuery(conceptoId)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Combinaciones completas de gasto (concepto, categoría, cuenta, forma de pago, proveedor,
    /// persona) más repetidas por el usuario, para ofrecerlas como accesos rápidos de un toque.
    /// </summary>
    [HttpGet("habituales")]
    public async Task<IActionResult> GetHabituales([FromQuery] int limit = 6)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetHabitualesGastosQuery(limit)
        {
            UsuarioId = usuarioId
        };

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
