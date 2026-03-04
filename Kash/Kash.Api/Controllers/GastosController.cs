using Kash.Application.Features.Gastos.Commands;
using Kash.Application.Features.Gastos.Queries;
using Kash.NuevaApi.Controllers.Base;
using Kash.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.NuevaApi.Controllers;

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
        // ✅ OPTIMIZACIÓN: Usamos el helper de la clase base
        var usuarioId = GetCurrentUserId();

        if (usuarioId is null)
        {
            // Retornamos un 401 usando el formato estandarizado
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado")));
        }

        var query = new GetGastosPagedListQuery(page, pageSize, searchTerm, sortColumn, sortOrder)
        {
            UsuarioId = usuarioId.Value
        };

        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetGastoByIdQuery(id);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGastoRequest request)
    {
        var userId = GetCurrentUserId();

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
            UsuarioId = userId!.Value
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
        // Nota: En Updates, generalmente no permitimos cambiar el UsuarioId (seguridad),
        // por lo que usamos el del request si viene, pero el Handler debería validar la propiedad.
        // Opcionalmente podrías forzar: command.UsuarioId = GetCurrentUserId();
        var userId = GetCurrentUserId();

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
            UsuarioId = userId!.Value
        };

        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteGastoCommand(id);
        var result = await _sender.Send(command);
        return HandleResult(result);
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
    Guid UsuarioId, // 🔥 CORREGIDO: Faltaba coma
                    // 🔥 NUEVO: Nombres opcionales para auto-creación de entidades
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