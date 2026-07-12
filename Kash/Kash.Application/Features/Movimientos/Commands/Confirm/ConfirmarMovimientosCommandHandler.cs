using Kash.Application.Features.Movimientos.Commands.Import.Models;
using Kash.Application.Interfaces;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using MediatR;

namespace Kash.Application.Features.Movimientos.Commands.Confirm;

/// <summary>
/// Crea en bloque los movimientos ya revisados por el usuario (auto-creando por nombre
/// cuenta/categoría/concepto/forma de pago). No deduplica: el usuario ya revisó.
/// </summary>
public sealed class ConfirmarMovimientosCommandHandler
    : IRequestHandler<ConfirmarMovimientosCommand, Result<ImportarMovimientosResult>>
{
    private readonly IMovimientoBulkCreator _bulkCreator;
    private readonly IUserContext _userContext;

    public ConfirmarMovimientosCommandHandler(
        IMovimientoBulkCreator bulkCreator,
        IUserContext userContext)
    {
        _bulkCreator = bulkCreator;
        _userContext = userContext;
    }

    public async Task<Result<ImportarMovimientosResult>> Handle(
        ConfirmarMovimientosCommand request,
        CancellationToken cancellationToken)
    {
        var usuarioId = _userContext.UserId;
        if (usuarioId is null)
            return Result.Failure<ImportarMovimientosResult>(Error.Unauthorized("Usuario no autenticado."));

        var movimientos = request.Movimientos
            .Select(m => new MovimientoACrear(
                EsGasto: m.Tipo.Equals("gasto", StringComparison.OrdinalIgnoreCase),
                Importe: m.Importe,
                Fecha: m.Fecha,
                Descripcion: m.Descripcion,
                CategoriaNombre: m.CategoriaNombre,
                ConceptoNombre: m.ConceptoNombre,
                CuentaNombre: m.CuentaNombre,
                FormaPagoNombre: m.FormaPagoNombre,
                ProveedorNombre: m.ProveedorNombre))
            .ToList();

        var resultado = await _bulkCreator.CrearAsync(usuarioId.Value, movimientos, cancellationToken);

        return Result.Success(new ImportarMovimientosResult(
            resultado.Gastos, resultado.Ingresos, 0, resultado.Fallidos, resultado.Errores));
    }
}
