using CsvHelper;
using CsvHelper.Configuration;
using Kash.Application.Features.Inversiones.Commands.Import.Models;
using System.Globalization;

namespace Kash.Application.Features.Inversiones.Commands.Import.Parsers;

/// <summary>
/// Parser para CSV de Interactive Brokers (IBKR).
/// Solo importa filas con Quantity/Cantidad mayor que 0 (compras).
/// Soporta cabeceras en ingles: TradeDate, Symbol, Description, Quantity, TradePrice, CurrencyPrimary, AssetCategory.
/// Soporta cabeceras en espanol: Fecha, Simbolo, Descripcion, Cantidad, Precio, Moneda, Categoria.
/// Soporta numeros en formato europeo (1.234,56) e internacional (1234.56).
/// </summary>
public sealed class InteractiveBrokersCsvParser : IInversionParser
{
    private static readonly string[] FormatosFecha =
        ["yyyyMMdd", "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"];

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
                // Cantidad: ingles o espanol
                if (!csv.TryGetField("Quantity", out string quantityStr) &&
                    !csv.TryGetField("Cantidad", out quantityStr))
                    throw new FormatException("Falta la columna Quantity/Cantidad");

                var quantityLimpio = quantityStr.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(quantityLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantidad) || cantidad <= 0)
                    continue; // Ventas: ignorar silenciosamente

                // Simbolo (ticker de Yahoo Finance, no necesita resolucion ISIN)
                if (!csv.TryGetField("Symbol", out string symbol) &&
                    !csv.TryGetField("Simbolo", out symbol))
                    throw new FormatException("Falta la columna Symbol/Simbolo");

                // Descripcion del activo
                if (!csv.TryGetField("Description", out string description) &&
                    !csv.TryGetField("Descripcion", out description))
                    description = symbol;

                // Precio
                if (!csv.TryGetField("TradePrice", out string priceStr) &&
                    !csv.TryGetField("Precio", out priceStr))
                    throw new FormatException("Falta la columna TradePrice/Precio");

                var precioLimpio = priceStr.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(precioLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) || precio <= 0)
                    throw new FormatException("Precio de compra invalido");

                // Moneda
                if (!csv.TryGetField("CurrencyPrimary", out string currency) &&
                    !csv.TryGetField("Moneda", out currency) &&
                    !csv.TryGetField("Currency", out currency))
                    throw new FormatException("Falta la columna CurrencyPrimary/Moneda");

                // Fecha
                if (!csv.TryGetField("TradeDate", out string dateStr) &&
                    !csv.TryGetField("Fecha", out dateStr))
                    throw new FormatException("Falta la columna TradeDate/Fecha");

                if (!DateOnly.TryParseExact(dateStr, FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                    throw new FormatException("Fecha con formato incorrecto");

                // Categoria del activo
                if (!csv.TryGetField("AssetCategory", out string category) &&
                    !csv.TryGetField("Categoria", out category))
                    category = string.Empty;

                var tipo = category.Trim().ToUpperInvariant() switch
                {
                    "STK"  => "accion",
                    "FUND" => "fondo",
                    "BOND" => "bono",
                    "CRYPTO" => "cripto",
                    _ => "otro"
                };

                var nombre = description.Length > 0 ? description : symbol;
                rows.Add(new InversionImportDto(nombre, symbol, tipo, cantidad, precio, currency, fecha, null, "Interactive Brokers"));
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorLinea(lineIndex, raw, ex.Message));
            }
        }

        return Task.FromResult(new ParseResult(rows, errors));
    }
}
