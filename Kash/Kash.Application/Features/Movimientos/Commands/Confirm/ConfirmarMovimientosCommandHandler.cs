using Kash.Application.Features.Gastos.Commands;
using Kash.Application.Features.Ingresos.Commands;
using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kash.Application.Features.Movimientos.Commands.Confirm;

/// <summary>
/// Crea cada movimiento revisado usando <see cref="CreateGastoCommand"/>/<see cref="CreateIngresoCommand"/>
/// (auto-crean por nombre cuenta/categoría/concepto/forma de pago). No deduplica: el usuario ya revisó.
/// </summary>
public sealed class ConfirmarMovimientosCommandHandler
    : IRequestHandler<ConfirmarMovimientosCommand, Result<ImportarMovimientosResult>>
{
    private readonly IMediator _mediator;
    private readonly IUserContext _userContext;
    private readonly ILogger<ConfirmarMovimientosCommandHandler> _logger;

    public ConfirmarMovimientosCommandHandler(
        IMediator mediator,
        IUserContext userContext,
        ILogger<ConfirmarMovimientosCommandHandler> logger)
    {
        _mediator = mediator;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<Result<ImportarMovimientosResult>> Handle(
        ConfirmarMovimientosCommand request,
        CancellationToken cancellationToken)
    {
        var usuarioId = _userContext.UserId;
        if (usuarioId is null)
            return Result.Failure<ImportarMovimientosResult>(Error.Unauthorized("Usuario no autenticado."));

        var errores = new List<MovimientoImportError>();
        int gastos = 0, ingresos = 0, fallidos = 0;

        foreach (var mov in request.Movimientos)
        {
            var esGasto = mov.Tipo.Equals("gasto", StringComparison.OrdinalIgnoreCase);

            var result = esGasto
                ? await _mediator.Send(CrearGasto(mov, usuarioId.Value), cancellationToken)
                : await _mediator.Send(CrearIngreso(mov, usuarioId.Value), cancellationToken);

            if (result.IsSuccess)
            {
                if (esGasto) gastos++;
                else ingresos++;
            }
            else
            {
                fallidos++;
                errores.Add(new MovimientoImportError(0, mov.Descripcion, result.Error.Message));
                _logger.LogWarning("No se pudo confirmar el movimiento '{Desc}': {Error}",
                    mov.Descripcion, result.Error.Message);
            }
        }

        return Result.Success(new ImportarMovimientosResult(gastos, ingresos, 0, fallidos, errores));
    }

    private static CreateGastoCommand CrearGasto(MovimientoConfirmarDto mov, Guid usuarioId) => new()
    {
        Importe = mov.Importe,
        Fecha = mov.Fecha,
        Descripcion = mov.Descripcion,
        CategoriaId = Guid.Empty,
        ConceptoId = Guid.Empty,
        ProveedorId = null,
        PersonaId = null,
        CuentaId = Guid.Empty,
        FormaPagoId = Guid.Empty,
        UsuarioId = usuarioId,
        CategoriaNombre = mov.CategoriaNombre,
        ConceptoNombre = mov.ConceptoNombre,
        CuentaNombre = mov.CuentaNombre,
        FormaPagoNombre = mov.FormaPagoNombre,
        ProveedorNombre = mov.ProveedorNombre
    };

    private static CreateIngresoCommand CrearIngreso(MovimientoConfirmarDto mov, Guid usuarioId) => new()
    {
        Importe = mov.Importe,
        Fecha = mov.Fecha,
        Descripcion = mov.Descripcion,
        CategoriaId = Guid.Empty,
        ConceptoId = Guid.Empty,
        ClienteId = null,
        PersonaId = null,
        CuentaId = Guid.Empty,
        FormaPagoId = Guid.Empty,
        UsuarioId = usuarioId,
        CategoriaNombre = mov.CategoriaNombre,
        ConceptoNombre = mov.ConceptoNombre,
        CuentaNombre = mov.CuentaNombre,
        FormaPagoNombre = mov.FormaPagoNombre
    };
}
