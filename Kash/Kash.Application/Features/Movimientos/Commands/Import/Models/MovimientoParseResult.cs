namespace Kash.Application.Features.Movimientos.Commands.Import.Models;

/// <summary>Resultado del parseo del fichero: filas válidas y errores por línea.</summary>
public sealed record MovimientoParseResult(
    List<MovimientoImportDto> Filas,
    List<MovimientoImportError> Errores);

/// <summary>Resultado final de la importación tras crear los movimientos.</summary>
public sealed record ImportarMovimientosResult(
    int GastosCreados,
    int IngresosCreados,
    int Duplicados,
    int Fallidos,
    List<MovimientoImportError> Errores);
