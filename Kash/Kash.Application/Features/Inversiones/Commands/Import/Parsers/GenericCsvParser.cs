using CsvHelper;
using CsvHelper.Configuration;
using Kash.Application.Features.Inversiones.Commands.Import.Models;
using System.Globalization;

namespace Kash.Application.Features.Inversiones.Commands.Import.Parsers;

/// <summary>
/// Parser para CSV genérico exportado desde Kash u otras fuentes compatibles.
/// Cabecera obligatoria (español o inglés):
///   nombre/name, ticker, tipo/type, cantidad/quantity, precio_compra/price,
///   moneda/currency, fecha_compra/date [, descripcion] [, plataforma]
/// Separador detectado automáticamente (, o ;).
/// Soporta números en formato europeo (1.234,56) e internacional (1234.56).
/// </summary>
public sealed class GenericCsvParser : IInversionParser
{
    private static readonly string[] FormatosFecha = ["yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"];
    private static readonly string[] TiposValidos  = ["etf", "accion", "cripto", "bono", "fondo", "mercado_privado", "otro"];

    public Task<ParseResult> ParseAsync(byte[] content, CancellationToken cancellationToken = default)
    {
        var rows   = new List<InversionImportDto>();
        var errors = new List<ImportErrorLinea>();
        var text   = System.Text.Encoding.UTF8.GetString(content);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter         = ParserHelpers.DetectDelimiter(text),
            HasHeaderRecord   = true,
            MissingFieldFound = null
        };

        using var reader = new StringReader(text);
        using var csv    = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        var lineIndex = 1;
        while (csv.Read())
        {
            lineIndex++;
            var raw = string.Join(config.Delimiter, csv.Parser.Record ?? []);
            try
            {
                // Columnas con alias español/inglés
                if (!csv.TryGetField("nombre", out string nombre) && !csv.TryGetField("name", out nombre))
                    throw new FormatException("Falta la columna nombre/name");

                if (!csv.TryGetField("ticker", out string ticker))
                    throw new FormatException("Falta la columna ticker");

                if (!csv.TryGetField("tipo", out string tipo) && !csv.TryGetField("type", out tipo))
                    throw new FormatException("Falta la columna tipo/type");

                if (!csv.TryGetField("cantidad", out string cantidadStr) && !csv.TryGetField("quantity", out cantidadStr))
                    throw new FormatException("Falta la columna cantidad/quantity");

                if (!csv.TryGetField("precio_compra", out string precioStr) && !csv.TryGetField("price", out precioStr))
                    throw new FormatException("Falta la columna precio_compra/price");

                if (!csv.TryGetField("moneda", out string moneda) && !csv.TryGetField("currency", out moneda))
                    throw new FormatException("Falta la columna moneda/currency");

                if (!csv.TryGetField("fecha_compra", out string fechaStr) && !csv.TryGetField("date", out fechaStr))
                    throw new FormatException("Falta la columna fecha_compra/date");

                // Limpiar y parsear números (soporta formato europeo: 1.234,56)
                var cantidadLimpia = cantidadStr.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(cantidadLimpia, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantidad) || cantidad <= 0)
                    throw new FormatException("Cantidad inválida o cero");

                var precioLimpio = precioStr.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(precioLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) || precio <= 0)
                    throw new FormatException("Precio de compra inválido");

                if (!DateOnly.TryParseExact(fechaStr, FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                    throw new FormatException("Fecha con formato incorrecto");

                var tipoNorm = tipo.Trim().ToLowerInvariant();
                if (!TiposValidos.Contains(tipoNorm))
                    throw new FormatException($"Tipo de activo desconocido: {tipo}");

                csv.TryGetField("descripcion", out string? descripcion);
                if (!csv.TryGetField("descripcion", out descripcion))
                    csv.TryGetField("description", out descripcion);

                csv.TryGetField("plataforma", out string? plataforma);
                if (!csv.TryGetField("plataforma", out plataforma))
                    csv.TryGetField("platform", out plataforma);

                rows.Add(new InversionImportDto(nombre, ticker, tipoNorm, cantidad, precio, moneda, fecha, descripcion, plataforma));
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorLinea(lineIndex, raw, ex.Message));
            }
        }

        return Task.FromResult(new ParseResult(rows, errors));
    }
}
