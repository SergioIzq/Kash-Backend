using Kash.Application.Features.Inversiones.Commands.Import.Models;
using Kash.Application.Interfaces;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace Kash.Application.Features.Inversiones.Commands.Import.Parsers;

/// <summary>
/// Parser para confirmaciones de compra en PDF de Trade Republic (España).
/// Adaptado para extraer datos cuando PdfPig devuelve texto sin espacios.
/// </summary>
public sealed class TradeRepublicPdfParser : IInversionParser
{
    private static readonly string[] FormatosFecha = ["dd.MM.yyyy", "dd/MM/yyyy", "yyyy-MM-dd"];

    // Fecha: "FECHA02.03.2026"
    private static readonly Regex FechaEtiquetaRegex = new(
        @"FECHA\s*(\d{2}\.\d{2}\.\d{4})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Fila completa de la tabla. Texto real:
    // "IMPORTECore S&P 500 USD (Acc)ISIN: IE00B5BMR0872,698559626,26 EUR1.690,00 EUR"
    private static readonly Regex FilaTablaRegex = new(
        @"IMPORTE\s*(.+?)ISIN:\s*([A-Z]{2}[A-Z0-9]{10})(\d+[,]\d{1,6}|\d+)([\d.]+[,]\d+)\s+(EUR|USD|GBP)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Fallback de fecha genérica
    private static readonly Regex FechaGenericaRegex = new(
        @"(\d{2}[./]\d{2}[./]\d{4})",
        RegexOptions.Compiled);

    private readonly IIsinResolverService _isinResolver;

    public TradeRepublicPdfParser(IIsinResolverService isinResolver)
        => _isinResolver = isinResolver;

    public async Task<ParseResult> ParseAsync(byte[] content, CancellationToken cancellationToken = default)
    {
        var rows = new List<InversionImportDto>();
        var errors = new List<ImportErrorLinea>();

        try
        {
            using var pdf = PdfDocument.Open(content);
            var rawText = string.Join("\n", pdf.GetPages().Select(p => p.Text));

            // Colapsar whitespace múltiple a un solo espacio, pero mantener la cadena pegada que devuelve PdfPig
            var text = Regex.Replace(rawText, @"\s+", " ").Trim();

            var esCompra = text.Contains("Kauf", StringComparison.OrdinalIgnoreCase) ||
                           text.Contains("Compra", StringComparison.OrdinalIgnoreCase) ||
                           text.Contains("Buy", StringComparison.OrdinalIgnoreCase) ||
                           text.Contains("PLAN DE INVERSI", StringComparison.OrdinalIgnoreCase) ||
                           text.Contains("LIQUIDACION", StringComparison.OrdinalIgnoreCase) ||
                           text.Contains("LIQUIDACIÓN", StringComparison.OrdinalIgnoreCase);

            if (!esCompra)
                return new ParseResult(rows, errors);

            // -- 1. EXTRAER FILA PRINCIPAL (Nombre, ISIN, Cantidad, Precio, Moneda) --
            var filaMatch = FilaTablaRegex.Match(text);
            if (!filaMatch.Success)
                throw new FormatException("No se pudo extraer la fila de la tabla. Verifica el patrón de texto aplastado.");

            var nombre = filaMatch.Groups[1].Value.Trim();
            var isin = filaMatch.Groups[2].Value;
            var cantidadStr = filaMatch.Groups[3].Value;
            var precioStr = filaMatch.Groups[4].Value;
            var moneda = filaMatch.Groups[5].Value.ToUpperInvariant();

            // -- 2. LIMPIEZA Y PARSEO DE NÚMEROS --
            var cantidadLimpia = cantidadStr.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(cantidadLimpia, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var cantidad) || cantidad <= 0)
                throw new FormatException($"Cantidad inválida o cero: {cantidadStr}");

            var precioLimpio = precioStr.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(precioLimpio, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var precio) || precio <= 0)
                throw new FormatException($"Precio de compra inválido: {precioStr}");

            // -- 3. FECHA --
            var fechaStr = string.Empty;

            var fechaEtiquetaMatch = FechaEtiquetaRegex.Match(text);
            if (fechaEtiquetaMatch.Success)
            {
                fechaStr = fechaEtiquetaMatch.Groups[1].Value;
            }
            else
            {
                var fechaGenericaMatch = FechaGenericaRegex.Match(text);
                if (fechaGenericaMatch.Success)
                    fechaStr = fechaGenericaMatch.Value.Replace("/", ".");
            }

            if (string.IsNullOrEmpty(fechaStr) ||
                !DateOnly.TryParseExact(fechaStr, FormatosFecha,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var fecha))
                throw new FormatException("Fecha con formato incorrecto o no encontrada");

            // -- 4. RESOLVER TICKER Y TIPO --
            var ticker = await _isinResolver.ResolveAsync(isin, cancellationToken);
            var tipo = ParserHelpers.InferirTipo(isin, nombre);

            rows.Add(new InversionImportDto(nombre, ticker, tipo, cantidad, precio, moneda, fecha, null, "Trade Republic"));
        }
        catch (Exception ex)
        {
            errors.Add(new ImportErrorLinea(1, "(pdf)", ex.Message));
        }

        return new ParseResult(rows, errors);
    }
}