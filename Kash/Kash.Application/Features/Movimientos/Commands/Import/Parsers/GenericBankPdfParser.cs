using Kash.Application.Features.Movimientos.Commands.Import.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Kash.Application.Features.Movimientos.Commands.Import.Parsers;

/// <summary>
/// Parser genérico de extractos bancarios en PDF. Reconstruye las líneas por coordenadas
/// (PdfPig suele devolver el texto pegado) y extrae cada movimiento con:
///   - un regex configurable (<see cref="ColumnMapping.LineRegex"/>) con grupos con nombre, o
///   - una heurística por defecto: fecha al inicio de línea + importe(s) numéricos.
/// Best-effort: los PDF son menos fiables que el CSV. Las líneas que no encajan se ignoran.
/// </summary>
public sealed class GenericBankPdfParser
{
    private static readonly string[] FormatosFechaPorDefecto =
        ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "dd.MM.yyyy", "MM/dd/yyyy"];

    // Fecha al inicio de línea: 01/02/2026, 1.2.26, 2026-02-01, etc.
    private static readonly Regex FechaInicioRegex = new(
        @"^\s*(?<fecha>\d{4}-\d{2}-\d{2}|\d{1,2}[/.\-]\d{1,2}[/.\-]\d{2,4})",
        RegexOptions.Compiled);

    // Importe monetario (con decimales obligatorios). Soporta miles con . , o espacio y signo/paréntesis.
    private static readonly Regex ImporteRegex = new(
        @"[-+]?\(?\s?\d{1,3}(?:[.\s]\d{3})*[.,]\d{1,2}\s?\)?|[-+]?\(?\d+[.,]\d{1,2}\)?",
        RegexOptions.Compiled);

    public Task<MovimientoParseResult> ParseAsync(byte[] content, ColumnMapping map, CancellationToken ct = default)
    {
        var filas = new List<MovimientoImportDto>();
        var errores = new List<MovimientoImportError>();
        var formatosFecha = map.FechaFormatos is { Length: > 0 } ? map.FechaFormatos : FormatosFechaPorDefecto;

        Regex? lineRegex = null;
        if (!string.IsNullOrWhiteSpace(map.LineRegex))
        {
            try
            {
                lineRegex = new Regex(map.LineRegex, RegexOptions.Compiled);
            }
            catch (ArgumentException ex)
            {
                return Task.FromResult(new MovimientoParseResult(filas,
                    [new MovimientoImportError(0, map.LineRegex, $"LineRegex inválido: {ex.Message}")]));
            }
        }

        List<string> lineas;
        try
        {
            lineas = ExtraerLineas(content);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new MovimientoParseResult(filas,
                [new MovimientoImportError(0, "(pdf)", $"No se pudo leer el PDF: {ex.Message}")]));
        }

        var lineIndex = map.SkipRows;
        foreach (var linea in lineas.Skip(map.SkipRows))
        {
            lineIndex++;
            if (string.IsNullOrWhiteSpace(linea)) continue;

            try
            {
                var mov = lineRegex is not null
                    ? ExtraerConRegex(linea, lineRegex, map, formatosFecha)
                    : ExtraerHeuristico(linea, map, formatosFecha);

                if (mov is not null)
                    filas.Add(mov);
            }
            catch (Exception ex)
            {
                errores.Add(new MovimientoImportError(lineIndex, linea, ex.Message));
            }
        }

        if (filas.Count == 0 && errores.Count == 0)
            errores.Add(new MovimientoImportError(0, "(pdf)",
                "No se detectó ningún movimiento. Ajusta 'lineRegex' o los formatos de fecha para este banco."));

        return Task.FromResult(new MovimientoParseResult(filas, errores));
    }

    // --- Extracción con regex configurable ---
    private static MovimientoImportDto? ExtraerConRegex(string linea, Regex regex, ColumnMapping map, string[] formatosFecha)
    {
        var m = regex.Match(linea);
        if (!m.Success) return null;

        var fechaStr = GrupoOpcional(m, "fecha");
        if (fechaStr is null) return null;
        var fecha = ParseFecha(fechaStr, formatosFecha);

        var descripcion = (GrupoOpcional(m, "concepto") ?? string.Empty).Trim();

        var cargoStr = GrupoOpcional(m, "cargo");
        var abonoStr = GrupoOpcional(m, "abono");
        if (cargoStr is not null || abonoStr is not null)
        {
            var tieneCargo = BankAmountParser.TryParse(cargoStr, map.DecimalSeparator, out var cargo) && cargo != 0;
            var tieneAbono = BankAmountParser.TryParse(abonoStr, map.DecimalSeparator, out var abono) && abono != 0;

            if (tieneCargo) return new MovimientoImportDto(fecha, descripcion, Math.Abs(cargo), TipoMovimiento.Gasto);
            if (tieneAbono) return new MovimientoImportDto(fecha, descripcion, Math.Abs(abono), TipoMovimiento.Ingreso);
            return null;
        }

        var importeStr = GrupoOpcional(m, "importe")
            ?? throw new FormatException("El regex no capturó ningún importe (grupos 'importe' o 'cargo'/'abono').");

        if (!BankAmountParser.TryParse(importeStr, map.DecimalSeparator, out var conSigno) || conSigno == 0)
            throw new FormatException($"Importe inválido: '{importeStr}'.");

        return new MovimientoImportDto(fecha, descripcion, Math.Abs(conSigno),
            conSigno < 0 ? TipoMovimiento.Gasto : TipoMovimiento.Ingreso);
    }

    // --- Heurística por defecto: fecha al inicio + importe(s) numéricos ---
    private static MovimientoImportDto? ExtraerHeuristico(string linea, ColumnMapping map, string[] formatosFecha)
    {
        var fechaMatch = FechaInicioRegex.Match(linea);
        if (!fechaMatch.Success) return null; // línea sin fecha => cabecera/pie/saldo, se ignora

        var fecha = ParseFecha(fechaMatch.Groups["fecha"].Value, formatosFecha);

        var resto = linea[fechaMatch.Length..];
        var importes = ImporteRegex.Matches(resto);
        if (importes.Count == 0) return null; // línea sin importe => se ignora

        var elegido = map.AmountPosition.Equals("last", StringComparison.OrdinalIgnoreCase)
            ? importes[^1]
            : importes[0];

        if (!BankAmountParser.TryParse(elegido.Value, map.DecimalSeparator, out var conSigno) || conSigno == 0)
            throw new FormatException($"Importe inválido: '{elegido.Value}'.");

        // Descripción = texto sin los importes detectados
        var descripcion = ImporteRegex.Replace(resto, " ");
        descripcion = Regex.Replace(descripcion, @"\s+", " ").Trim();

        return new MovimientoImportDto(fecha, descripcion, Math.Abs(conSigno),
            conSigno < 0 ? TipoMovimiento.Gasto : TipoMovimiento.Ingreso);
    }

    private static string? GrupoOpcional(Match m, string nombre)
    {
        var g = m.Groups[nombre];
        return g.Success && !string.IsNullOrWhiteSpace(g.Value) ? g.Value : null;
    }

    private static DateTime ParseFecha(string fechaStr, string[] formatosFecha)
    {
        fechaStr = fechaStr.Trim();
        if (DateTime.TryParseExact(fechaStr, formatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
            || DateTime.TryParse(fechaStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            return fecha;
        throw new FormatException($"Fecha con formato no reconocido: '{fechaStr}'.");
    }

    /// <summary>Reconstruye las líneas de todas las páginas agrupando las palabras por su línea base (Y).</summary>
    private static List<string> ExtraerLineas(byte[] content)
    {
        var resultado = new List<string>();
        using var pdf = PdfDocument.Open(content);

        foreach (var page in pdf.GetPages())
        {
            var words = page.GetWords().ToList();
            if (words.Count == 0) continue;

            // Agrupar por línea base redondeada (tolerancia ~2 pt) y ordenar de arriba a abajo.
            var lineas = words
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom / 2.0))
                .OrderByDescending(g => g.Key)
                .Select(g => string.Join(" ", g
                    .OrderBy(w => w.BoundingBox.Left)
                    .Select(w => w.Text)));

            resultado.AddRange(lineas);
        }

        return resultado;
    }
}
