using System.Globalization;

namespace Kash.Application.Features.Movimientos.Commands.Import;

/// <summary>
/// Clave canónica de deduplicación de movimientos, compartida entre el importador (que la calcula
/// para cada fila del fichero) y el checker de duplicados (que la calcula para las filas ya
/// existentes en base de datos). Deben producir exactamente el mismo formato.
/// </summary>
public static class MovimientoDedupKey
{
    public static string Construir(bool esGasto, DateTime fecha, decimal importe, string? descripcion) =>
        $"{(esGasto ? "Gasto" : "Ingreso")}|{fecha:yyyy-MM-dd}|{importe.ToString("0.00", CultureInfo.InvariantCulture)}|{descripcion ?? string.Empty}";
}
