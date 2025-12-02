using AhorroLand.Application.Features.Gastos.Commands;
using AhorroLand.Application.Features.Gastos.Queries;
using AhorroLand.NuevaApi.Controllers.Base;
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Error y Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AhorroLand.NuevaApi.Controllers;

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
    [HttpGet] // Estandarizado a la raíz (GET /api/gastos)
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
        // Asignación inteligente de UsuarioId
        var usuarioId = request.UsuarioId != Guid.Empty ? request.UsuarioId : GetCurrentUserId() ?? Guid.Empty;

        var command = new CreateGastoCommand
        {
            Importe = request.Importe,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            ConceptoId = request.ConceptoId,
            ProveedorId = request.ProveedorId,
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGastoRequest request)
    {
        // Nota: En Updates, generalmente no permitimos cambiar el UsuarioId (seguridad),
        // por lo que usamos el del request si viene, pero el Handler debería validar la propiedad.
        // Opcionalmente podrías forzar: command.UsuarioId = GetCurrentUserId();

        var command = new UpdateGastoCommand
        {
            Id = id,
            Importe = request.Importe,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CategoriaId = request.CategoriaId,
            ConceptoId = request.ConceptoId,
            ProveedorId = request.ProveedorId,
            PersonaId = request.PersonaId,
            CuentaId = request.CuentaId,
            FormaPagoId = request.FormaPagoId,
            UsuarioId = request.UsuarioId
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
    Guid ProveedorId,
    Guid PersonaId,
    Guid CuentaId,
    Guid FormaPagoId,
    Guid UsuarioId
);

public record UpdateGastoRequest(
    decimal Importe,
    DateTime Fecha,
    string? Descripcion,
    Guid CategoriaId,
    Guid ConceptoId,
    Guid ProveedorId,
    Guid PersonaId,
    Guid CuentaId,
    Guid FormaPagoId,
    Guid UsuarioId
);