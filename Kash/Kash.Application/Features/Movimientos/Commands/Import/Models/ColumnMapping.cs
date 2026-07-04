namespace Kash.Application.Features.Movimientos.Commands.Import.Models;

/// <summary>
/// Configuración que describe cómo interpretar el CSV de CUALQUIER banco.
/// El usuario indica qué columna es la fecha, el concepto y el importe.
/// Las columnas se identifican por NOMBRE de cabecera o por ÍNDICE (0-based).
/// </summary>
public sealed record ColumnMapping
{
    // --- Estructura del fichero ---

    /// <summary>Separador de campos. Si es null se autodetecta (',', ';' o tabulador).</summary>
    public string? Delimiter { get; init; }

    /// <summary>Indica si la primera fila (tras SkipRows) es la cabecera con los nombres de columna.</summary>
    public bool HasHeader { get; init; } = true;

    /// <summary>Filas iniciales del banco a descartar antes de la cabecera/datos (títulos, saldos, etc.).</summary>
    public int SkipRows { get; init; } = 0;

    // --- Columnas (nombre de cabecera o índice 0-based) ---

    public required string FechaColumn { get; init; }
    public required string ConceptoColumn { get; init; }

    /// <summary>Columna de importe con signo (negativo = gasto, positivo = ingreso).</summary>
    public string? ImporteColumn { get; init; }

    /// <summary>Alternativa a ImporteColumn: columna de cargos (gastos).</summary>
    public string? CargoColumn { get; init; }

    /// <summary>Alternativa a ImporteColumn: columna de abonos (ingresos).</summary>
    public string? AbonoColumn { get; init; }

    // --- Formatos ---

    /// <summary>Formatos de fecha aceptados. Si es null se prueban los formatos por defecto.</summary>
    public string[]? FechaFormatos { get; init; }

    /// <summary>Cómo interpretar el separador decimal: "auto" | "coma" | "punto".</summary>
    public string DecimalSeparator { get; init; } = "auto";

    // --- Específico de PDF ---

    /// <summary>
    /// (Solo PDF) Regex opcional con grupos con nombre para extraer cada línea de movimiento.
    /// Grupos soportados: (?&lt;fecha&gt;), (?&lt;concepto&gt;) y (?&lt;importe&gt;) — o (?&lt;cargo&gt;)/(?&lt;abono&gt;).
    /// Si es null se usa una heurística: fecha al inicio de línea + importe(s) al final.
    /// </summary>
    public string? LineRegex { get; init; }

    /// <summary>
    /// (Solo PDF, heurística por defecto) Cuando una línea tiene varios importes (p.ej. importe + saldo),
    /// cuál tomar como movimiento: "first" (por defecto) o "last".
    /// </summary>
    public string AmountPosition { get; init; } = "first";

    // --- Valores por defecto de clasificación (permiten importar sin teclear nada) ---

    /// <summary>Cuenta a la que pertenece este extracto. Se crea si no existe.</summary>
    public required string CuentaNombre { get; init; }

    public string FormaPagoNombre { get; init; } = "Transferencia";
    public string CategoriaGastoNombre { get; init; } = "Sin clasificar";
    public string CategoriaIngresoNombre { get; init; } = "Sin clasificar";
    public string ConceptoNombre { get; init; } = "Importado del banco";
}
