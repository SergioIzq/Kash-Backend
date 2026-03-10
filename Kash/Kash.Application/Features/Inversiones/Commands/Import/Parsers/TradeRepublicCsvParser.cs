using CsvHelper;
using CsvHelper.Configuration;
using Kash.Application.Features.Inversiones.Commands.Import.Models;
using Kash.Application.Interfaces;
using System.Globalization;

namespace Kash.Application.Features.Inversiones.Commands.Import.Parsers;

/// <summary>
/// Parser para CSV de Trade Republic (Adaptado a España/Multi-idioma).
/// Extrae operaciones de compra o planes de inversión.
/// </summary>
public sealed class TradeRepublicCsvParser : IInversionParser
{
    private readonly IIsinResolverService _isinResolver;

    public TradeRepublicCsvParser(IIsinResolverService isinResolver)
        => _isinResolver = isinResolver;

    public async Task<ParseResult> ParseAsync(byte[] content, CancellationToken cancellationToken = default)
    {
        var rows = new List<InversionImportDto>();
        var errors = new List<ImportErrorLinea>();
        var text = System.Text.Encoding.UTF8.GetString(content);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ParserHelpers.DetectDelimiter(text), // Habitualmente ';' en los CSV europeos
            HasHeaderRecord = true,
            MissingFieldFound = null // Evita que CsvHelper lance excepciones si falta una cabecera exacta
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
                // 1. Obtener tipo de operación (soportando español, inglés y alemán)
                if (!csv.TryGetField("Tipo", out string typ) &&
                    !csv.TryGetField("Type", out typ) &&
                    !csv.TryGetField("Typ", out typ))
                {
                    typ = string.Empty;
                }

                // Filtrar solo compras o ejecuciones de planes de inversión
                bool esCompra = typ.Equals("Compra", StringComparison.OrdinalIgnoreCase) ||
                                typ.Contains("Plan de inversi", StringComparison.OrdinalIgnoreCase) ||
                                typ.Equals("Kauf", StringComparison.OrdinalIgnoreCase) ||
                                typ.Equals("Buy", StringComparison.OrdinalIgnoreCase);

                if (!esCompra)
                    continue;

                // 2. Extraer campos con compatibilidad multi-idioma
                var isin = csv.GetField("ISIN") ?? throw new FormatException("Falta la columna ISIN");

                if (!csv.TryGetField("Descripción", out string nombre) && !csv.TryGetField("Nombre", out nombre) && !csv.TryGetField("Name", out nombre))
                    throw new FormatException("Falta la columna Descripción/Nombre");

                if (!csv.TryGetField("Cantidad", out string stucke) && !csv.TryGetField("Shares", out stucke) && !csv.TryGetField("Stücke", out stucke))
                    throw new FormatException("Falta la columna Cantidad");

                if (!csv.TryGetField("Precio", out string kurs) && !csv.TryGetField("Cotización", out kurs) && !csv.TryGetField("Price", out kurs) && !csv.TryGetField("Kurs", out kurs))
                    throw new FormatException("Falta la columna Precio/Cotización");

                if (!csv.TryGetField("Divisa", out string wahrung) && !csv.TryGetField("Moneda", out wahrung) && !csv.TryGetField("Currency", out wahrung) && !csv.TryGetField("Währung", out wahrung))
                    throw new FormatException("Falta la columna Divisa/Moneda");

                if (!csv.TryGetField("Fecha", out string datum) && !csv.TryGetField("Date", out datum) && !csv.TryGetField("Datum", out datum))
                    throw new FormatException("Falta la columna Fecha");

                // 3. Limpiar y parsear números (soportando formato europeo ej: 1.690,00)
                var cantidadLimpia = stucke.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(cantidadLimpia, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantidad) || cantidad <= 0)
                    throw new FormatException("Cantidad inválida o cero");

                var precioLimpio = kurs.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(precioLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) || precio <= 0)
                    throw new FormatException("Precio de compra inválido");

                // 4. Parsear fecha (soportando varios formatos habituales en CSVs)
                string[] formatosFecha = { "dd.MM.yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };
                if (!DateOnly.TryParseExact(datum, formatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                    throw new FormatException("Fecha con formato incorrecto");

                // 5. Resolver y añadir
                var ticker = await _isinResolver.ResolveAsync(isin, cancellationToken);
                var tipo = ParserHelpers.InferirTipo(isin, nombre);

                rows.Add(new InversionImportDto(nombre, ticker, tipo, cantidad, precio, wahrung, fecha, null, "Trade Republic"));
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorLinea(lineIndex, raw, ex.Message));
            }
        }

        return new ParseResult(rows, errors);
    }
}