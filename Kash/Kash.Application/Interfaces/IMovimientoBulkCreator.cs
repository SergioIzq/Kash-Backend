using Kash.Application.Features.Movimientos.Commands.Import.Models;
using SergioIzq.Application.Kernel.Interfaces;

namespace Kash.Application.Interfaces;

/// <summary>Un movimiento a crear, con sus dependencias expresadas por nombre (se auto-crean si no existen).</summary>
public sealed record MovimientoACrear(
    bool EsGasto,
    decimal Importe,
    DateTime Fecha,
    string? Descripcion,
    string CategoriaNombre,
    string ConceptoNombre,
    string CuentaNombre,
    string FormaPagoNombre,
    string? ProveedorNombre);

/// <summary>Resultado de una creación en bloque: cuántos se crearon, cuántos fallaron y por qué.</summary>
public sealed record BulkCreateResult(int Gastos, int Ingresos, int Fallidos, List<MovimientoImportError> Errores);

/// <summary>
/// Crea muchos gastos/ingresos de una sola vez de forma eficiente: resuelve las dependencias
/// compartidas (cuenta, categoría, concepto, forma de pago, proveedor) una única vez por nombre,
/// actualiza el saldo de cada cuenta de forma agregada y persiste todo en una sola transacción,
/// invalidando la caché una sola vez. Reutilizado por la importación de extractos y por la
/// confirmación de movimientos revisados.
/// </summary>
public interface IMovimientoBulkCreator : IApplicationService
{
    Task<BulkCreateResult> CrearAsync(
        Guid usuarioId,
        IReadOnlyList<MovimientoACrear> movimientos,
        CancellationToken cancellationToken = default);
}
