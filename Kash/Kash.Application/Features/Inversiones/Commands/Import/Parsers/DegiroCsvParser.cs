using CsvHelper;
using CsvHelper.Configuration;
using Kash.Application.Features.Inversiones.Commands.Import.Models;
using Kash.Application.Interfaces;
using System.Globalization;

namespace Kash.Application.Features.Inversiones.Commands.Import.Parsers;

/// <summary>
/// Parser para CSV de DEGIRO (version espanola e inglesa).
/// Solo importa filas con Numero/Shares mayor que 0 (operaciones de compra).
/// Soporta cabeceras en espanol: Fecha, Producto, ISIN, Numero, Precio, Divisa local.
/// Soporta cabeceras en ingles: Date, Product, ISIN, Quantity, Price, Local value currency.
/// Soporta numeros en formato europeo (1.234,56) e internacional (1234.56).
/// </summary>
public sealed class DegiroCsvParser : IInversionParser
{
    private static readonly string[] FormatosFecha = ["dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "dd.MM.yyyy"];

    private readonly IIsinResolverService _isinResolver;

    public DegiroCsvParser(IIsinResolverService isinResolver)
        => _isinResolver = isinResolver;

    public async Task<ParseResult> ParseAsync(byte[] content, CancellationToken cancellationToken = default)
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
                // Numero de acciones: espanol, ingles
                if (!csv.TryGetField("Numero", out string numeroStr) &&
                    !csv.TryGetField("Shares", out numeroStr) &&
                    !csv.TryGetField("Quantity", out numeroStr))
                    throw new FormatException("Falta la columna Numero/Shares/Quantity");

                var numeroLimpio = numeroStr.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(numeroLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantidad) || cantidad <= 0)
                    continue; // Ventas o filas de liquidez: ignorar silenciosamente

                // Nombre del activo
                if (!csv.TryGetField("Producto", out string nombre) && !csv.TryGetField("Product", out nombre))
                    throw new FormatException("Falta la columna Producto/Product");

                // ISIN
                var isin = csv.GetField("ISIN") ?? throw new FormatException("Falta la columna ISIN");

                // Precio
                if (!csv.TryGetField("Precio", out string precioStr) && !csv.TryGetField("Price", out precioStr))
                    throw new FormatException("Falta la columna Precio/Price");

                var precioLimpio = precioStr.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(precioLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) || precio <= 0)
                    throw new FormatException("Precio de compra invalido");

                // Moneda
                if (!csv.TryGetField("Divisa local", out string moneda) &&
                    !csv.TryGetField("Local value currency", out moneda) &&
                    !csv.TryGetField("Currency", out moneda))
                    throw new FormatException("Falta la columna de moneda");

                // Fecha
                if (!csv.TryGetField("Fecha", out string fechaStr) && !csv.TryGetField("Date", out fechaStr))
                    throw new FormatException("Falta la columna Fecha/Date");

                if (!DateOnly.TryParseExact(fechaStr, FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                    throw new FormatException("Fecha con formato incorrecto");

                var ticker = await _isinResolver.ResolveAsync(isin, cancellationToken);
                var tipo   = ParserHelpers.InferirTipo(isin, nombre);

                rows.Add(new InversionImportDto(nombre, ticker, tipo, cantidad, precio, moneda, fecha, null, "DEGIRO"));
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorLinea(lineIndex, raw, ex.Message));
            }
        }

        return new ParseResult(rows, errors);
    }
}
