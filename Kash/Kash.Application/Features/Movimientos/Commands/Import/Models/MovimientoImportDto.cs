namespace Kash.Application.Features.Movimientos.Commands.Import.Models;

public enum TipoMovimiento
{
    Gasto,
    Ingreso
}

/// <summary>
/// Una línea del extracto ya interpretada. El importe es siempre positivo;
/// el signo original queda representado en <see cref="Tipo"/>.
/// </summary>
public sealed record MovimientoImportDto(
    DateTime Fecha,
    string Descripcion,
    decimal Importe,
    TipoMovimiento Tipo);
