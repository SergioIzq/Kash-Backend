using CsvHelper;
using CsvHelper.Configuration;
using Kash.Application.Features.Movimientos.Commands.Import.Models;
using System.Globalization;

namespace Kash.Application.Features.Movimientos.Commands.Import.Parsers;

/// <summary>
/// Parser genérico y configurable de extractos bancarios en CSV.
/// Válido para cualquier banco: el <see cref="ColumnMapping"/> indica qué columna
/// es fecha/concepto/importe (por nombre de cabecera o por índice), el separador,
/// el formato de fecha y cómo interpretar los decimales.
/// </summary>
public sealed class GenericBankCsvParser
{
    private static readonly string[] FormatosFechaPorDefecto =
        ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "dd.MM.yyyy", "MM/dd/yyyy"];

    public Task<MovimientoParseResult> ParseAsync(byte[] content, ColumnMapping map, CancellationToken ct = default)
    {
        var filas = new List<MovimientoImportDto>();
        var errores = new List<MovimientoImportError>();

        var text = DecodeText(content);
        var delimiter = string.IsNullOrEmpty(map.Delimiter) ? DetectDelimiter(text) : map.Delimiter;
        var formatosFecha = map.FechaFormatos is { Length: > 0 } ? map.FechaFormatos : FormatosFechaPorDefecto;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = false,   // leemos crudo para soportar índices, SkipRows y ficheros sin cabecera
            MissingFieldFound = null,
            BadDataFound = null
        };

        var todos = new List<string[]>();
        using (var reader = new StringReader(text))
        using (var csv = new CsvReader(reader, config))
        {
            while (csv.Read())
                todos.Add(csv.Parser.Record ?? []);
        }

        var filasUtiles = todos.Skip(map.SkipRows).ToList();
        if (filasUtiles.Count == 0)
            return Task.FromResult(new MovimientoParseResult(filas,
                [new MovimientoImportError(0, "", "El fichero no contiene datos tras aplicar SkipRows.")]));

        string[]? header = null;
        var filasDatos = filasUtiles;
        var headerOffset = 0;
        if (map.HasHeader)
        {
            // Muchos bancos añaden filas de título/fecha antes de la cabecera real.
            // Si las columnas se piden por nombre, localizamos la cabecera automáticamente.
            headerOffset = LocalizarCabecera(filasUtiles, map);
            header = filasUtiles[headerOffset];
            filasDatos = filasUtiles.Skip(headerOffset + 1).ToList();
            headerOffset++;
        }

        int? idxFecha = ResolveColumn(map.FechaColumn, header);
        int? idxConcepto = ResolveColumn(map.ConceptoColumn, header);
        int? idxImporte = ResolveColumn(map.ImporteColumn, header);
        int? idxCargo = ResolveColumn(map.CargoColumn, header);
        int? idxAbono = ResolveColumn(map.AbonoColumn, header);

        // Validación del mapeo (errores globales, línea 0)
        if (idxFecha is null)
            errores.Add(new MovimientoImportError(0, "", $"No se encontró la columna de fecha: '{map.FechaColumn}'."));
        if (idxConcepto is null)
            errores.Add(new MovimientoImportError(0, "", $"No se encontró la columna de concepto: '{map.ConceptoColumn}'."));
        if (idxImporte is null && idxCargo is null && idxAbono is null)
            errores.Add(new MovimientoImportError(0, "", "Debes indicar una columna de importe, o columnas de cargo/abono."));

        if (errores.Count > 0)
            return Task.FromResult(new MovimientoParseResult(filas, errores));

        var lineaBase = map.SkipRows + headerOffset;
        for (var r = 0; r < filasDatos.Count; r++)
        {
            var record = filasDatos[r];
            var lineIndex = lineaBase + r + 1;

            if (record.Length == 0 || record.All(string.IsNullOrWhiteSpace))
                continue; // línea vacía

            var raw = string.Join(delimiter, record);
            try
            {
                var fechaStr = Field(record, idxFecha)
                    ?? throw new FormatException("Falta el valor de la fecha.");

                if (!DateTime.TryParseExact(fechaStr.Trim(), formatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
                    && !DateTime.TryParse(fechaStr.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                    throw new FormatException($"Fecha con formato no reconocido: '{fechaStr}'.");

                var descripcion = (Field(record, idxConcepto) ?? string.Empty).Trim();

                decimal importe;
                TipoMovimiento tipo;

                if (idxCargo.HasValue || idxAbono.HasValue)
                {
                    var tieneCargo = BankAmountParser.TryParse(Field(record, idxCargo), map.DecimalSeparator, out var cargo) && cargo != 0;
                    var tieneAbono = BankAmountParser.TryParse(Field(record, idxAbono), map.DecimalSeparator, out var abono) && abono != 0;

                    if (tieneCargo)
                    {
                        importe = Math.Abs(cargo);
                        tipo = TipoMovimiento.Gasto;
                    }
                    else if (tieneAbono)
                    {
                        importe = Math.Abs(abono);
                        tipo = TipoMovimiento.Ingreso;
                    }
                    else
                    {
                        throw new FormatException("La línea no tiene importe en cargo ni en abono.");
                    }
                }
                else
                {
                    var importeStr = Field(record, idxImporte)
                        ?? throw new FormatException("Falta el valor del importe.");

                    if (!BankAmountParser.TryParse(importeStr, map.DecimalSeparator, out var conSigno) || conSigno == 0)
                        throw new FormatException($"Importe inválido o cero: '{importeStr}'.");

                    tipo = conSigno < 0 ? TipoMovimiento.Gasto : TipoMovimiento.Ingreso;
                    importe = Math.Abs(conSigno);
                }

                filas.Add(new MovimientoImportDto(fecha, descripcion, importe, tipo));
            }
            catch (Exception ex)
            {
                errores.Add(new MovimientoImportError(lineIndex, raw, ex.Message));
            }
        }

        return Task.FromResult(new MovimientoParseResult(filas, errores));
    }

    /// <summary>
    /// Localiza la fila de cabecera real dentro de las primeras filas: la primera que contiene
    /// los nombres de columna pedidos. Así se saltan los títulos/fechas que el banco añade arriba.
    /// Si las columnas se piden por índice, o no se encuentra, devuelve 0 (primera fila).
    /// </summary>
    private static int LocalizarCabecera(List<string[]> filas, ColumnMapping map)
    {
        var buscar = new[] { map.FechaColumn, map.ConceptoColumn, map.ImporteColumn, map.CargoColumn, map.AbonoColumn }
            .Where(s => !string.IsNullOrWhiteSpace(s) && !EsIndice(s))
            .Select(s => s!.Trim())
            .ToArray();

        if (buscar.Length == 0)
            return 0; // columnas por índice: no hay nombres que localizar

        var limite = Math.Min(filas.Count, 25);
        for (var i = 0; i < limite; i++)
        {
            var celdas = filas[i];
            var todasPresentes = buscar.All(nombre =>
                celdas.Any(c => c.Trim().Equals(nombre, StringComparison.OrdinalIgnoreCase)));

            if (todasPresentes)
                return i;
        }

        return 0; // no encontrada: usar la primera fila como cabecera
    }

    private static bool EsIndice(string? spec)
        => !string.IsNullOrWhiteSpace(spec)
           && int.TryParse(spec.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out _);

    private static string? Field(string[] record, int? index)
        => index.HasValue && index.Value >= 0 && index.Value < record.Length
            ? record[index.Value]
            : null;

    /// <summary>Resuelve una columna a su índice: si es numérica se usa como índice, si no se busca en la cabecera.</summary>
    private static int? ResolveColumn(string? spec, string[]? header)
    {
        if (string.IsNullOrWhiteSpace(spec)) return null;
        spec = spec.Trim();

        if (int.TryParse(spec, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idx))
            return idx;

        if (header is not null)
        {
            for (var i = 0; i < header.Length; i++)
                if (header[i].Trim().Equals(spec, StringComparison.OrdinalIgnoreCase))
                    return i;
        }

        return null; // nombre no encontrado en la cabecera
    }

    private static string DetectDelimiter(string text)
    {
        var firstLine = text.Split('\n', 2)[0];
        if (firstLine.Contains('\t')) return "\t";
        if (firstLine.Contains(';')) return ";";
        return ",";
    }

    /// <summary>Decodifica UTF-8 (con o sin BOM) y hace fallback a Latin1, común en bancos españoles.</summary>
    private static string DecodeText(byte[] content)
    {
        var text = System.Text.Encoding.UTF8.GetString(content);
        if (text.Contains('�')) // apareció el carácter de reemplazo => no era UTF-8
            text = System.Text.Encoding.Latin1.GetString(content);
        return text;
    }
}
