using CsvHelper;
using CsvHelper.Configuration;
using Kash.Application.Features.Inversiones.Commands.Import.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kash.Application.Features.Inversiones.Commands.Import.Parsers;

/// <summary>
/// Parser para CSV de Binance (historial de operaciones spot).
/// Solo importa filas con Side/Lado == BUY/COMPRA.
/// Soporta cabeceras en ingles: Date(UTC), Pair, Side, Price, Executed, Amount, Fee.
/// Soporta cabeceras en espanol: Fecha(UTC), Par, Lado, Precio, Ejecutado, Importe, Comision.
/// Extrae el simbolo cripto del campo Executed/Ejecutado (ej: "0.001 BTC" => ticker "BTC-USD").
/// Normaliza USDT a USD.
/// </summary>
public sealed class BinanceCsvParser : IInversionParser
{
    private static readonly Regex CryptoAmountRegex = new(@"(\d+\.?\d*)\s*([A-Z]+)", RegexOptions.Compiled);

    private static readonly string[] FormatosFecha =
        ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy"];

    public Task<ParseResult> ParseAsync(byte[] content, CancellationToken cancellationToken = default)
    {
        var rows = new List<InversionImportDto>();
        var errors = new List<ImportErrorLinea>();
        var text = System.Text.Encoding.UTF8.GetString(content);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ParserHelpers.DetectDelimiter(text),
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using var reader = new StringReader(text);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        var lineIndex = 1;
        while (csv.Read())
        {
            lineIndex++;
            var raw = string.Join(config.Delimiter, csv.Parser.Record ?? []);
            try
            {
                // Lado de la operacion: solo compras
                if (!csv.TryGetField("Side", out string? side) &&
                    !csv.TryGetField("Lado", out side))
                    throw new FormatException("Falta la columna Side/Lado");

                if (!side!.Equals("BUY", StringComparison.OrdinalIgnoreCase) &&
                    !side.Equals("COMPRA", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Par de trading (ej: BTCUSDT)
                if (!csv.TryGetField("Pair", out string? pair) && !csv.TryGetField("Par", out pair))
                    throw new FormatException("Falta la columna Pair/Par");

                // Precio
                if (!csv.TryGetField("Price", out string? priceStr) && !csv.TryGetField("Precio", out priceStr))
                    throw new FormatException("Falta la columna Price/Precio");

                var precioLimpio = priceStr!.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(precioLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) || precio <= 0)
                    throw new FormatException("Precio de compra invalido");

                // Ejecutado (ej: "0.001 BTC" o "0.001BTC")
                if (!csv.TryGetField("Executed", out string? executedStr) &&
                    !csv.TryGetField("Ejecutado", out executedStr))
                    throw new FormatException("Falta la columna Executed/Ejecutado");

                var executedMatch = CryptoAmountRegex.Match(executedStr!);
                if (!executedMatch.Success)
                    throw new FormatException("Cantidad invalida o cero");

                var cantidadLimpia = executedMatch.Groups[1].Value.Replace(",", ".");
                if (!decimal.TryParse(cantidadLimpia, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantidad) || cantidad <= 0)
                    throw new FormatException("Cantidad invalida o cero");

                var cryptoSymbol = executedMatch.Groups[2].Value;
                var ticker = $"{cryptoSymbol}-USD";

                // Importe para extraer la moneda (ej: "65.00 USDT")
                if (!csv.TryGetField("Amount", out string? amountStr) &&
                    !csv.TryGetField("Importe", out amountStr))
                    amountStr = string.Empty;

                var monedaRaw = CryptoAmountRegex.Match(amountStr!).Groups[2].Value;
                var moneda = monedaRaw switch { "USDT" => "USD", "" => "USD", _ => monedaRaw };

                // Fecha
                if (!csv.TryGetField("Date(UTC)", out string? dateStr) &&
                    !csv.TryGetField("Fecha(UTC)", out dateStr) &&
                    !csv.TryGetField("Date", out dateStr) &&
                    !csv.TryGetField("Fecha", out dateStr))
                    throw new FormatException("Falta la columna de fecha");

                if (!DateTime.TryParseExact(dateStr, FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                    throw new FormatException("Fecha con formato incorrecto");

                var fecha = DateOnly.FromDateTime(dateTime);
                rows.Add(new InversionImportDto(pair!, ticker, "cripto", cantidad, precio, moneda, fecha, null, "Binance"));
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorLinea(lineIndex, raw, ex.Message));
            }
        }

        return Task.FromResult(new ParseResult(rows, errors));
    }
}
