using Kash.Application.Features.Ingresos.Commands;
using Kash.Application.Features.Ingresos.Queries;
using Kash.Application.Features.Ingresos.Queries.GetExcel;
using Kash.Application.Features.Ingresos.Queries.GetPeriodo;
using Kash.Application.Features.Ingresos.Queries.Habituales;
using Kash.Application.Features.Ingresos.Queries.Sugerencia;
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

    /// <summary>
    /// Obtiene los Ingresos del usuario cuya fecha de transacción cae dentro de un rango indicado,
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

        var query = new GetIngresosPorPeriodoQuery(usuarioId, fechaInicio, fechaFin, page, pageSize);

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Genera y descarga un Excel con los Ingresos del usuario que cumplen los filtros indicados
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
        [FromQuery] Guid[]? clienteIds,
        [FromQuery] Guid[]? personaIds,
        CancellationToken cancellationToken)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetIngresosExcelQuery(usuarioId, fechaInicio, fechaFin, searchTerm, conceptoIds, categoriaIds, clienteIds, personaIds);
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
        var query = new GetIngresoByIdQuery(id);
        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Obtiene la combinación (cuenta, forma de pago, importe, cliente, persona) del
    /// ingreso más reciente registrado por el usuario para un concepto dado, para pre-rellenar
    /// un alta nueva. Devuelve una lista vacía si el concepto no tiene ingresos previos.
    /// </summary>
    [HttpGet("sugerencia")]
    public async Task<IActionResult> GetSugerencia([FromQuery] Guid conceptoId)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetSugerenciaIngresoQuery(conceptoId)
        {
            UsuarioId = usuarioId
        };

        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Combinaciones completas de ingreso (concepto, categoría, cuenta, forma de pago, cliente,
    /// persona) más repetidas por el usuario, para ofrecerlas como accesos rápidos de un toque.
    /// </summary>
    [HttpGet("habituales")]
    public async Task<IActionResult> GetHabituales([FromQuery] int limit = 6)
    {
        if (RequireCurrentUserId(out var usuarioId) is { } unauthorized) return unauthorized;

        var query = new GetHabitualesIngresosQuery(limit)
        {
            UsuarioId = usuarioId
        };

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
