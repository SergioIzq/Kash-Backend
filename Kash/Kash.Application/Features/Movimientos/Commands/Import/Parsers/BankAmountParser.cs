using System.Globalization;

namespace Kash.Application.Features.Movimientos.Commands.Import.Parsers;

/// <summary>
/// Parseo robusto de importes de extractos bancarios. Soporta símbolos de moneda,
/// separadores de miles, formato europeo (1.234,56), internacional (1,234.56),
/// signos negativos y contabilidad entre paréntesis (1.234,56).
/// </summary>
internal static class BankAmountParser
{
    /// <param name="mode">"auto" | "coma" (europeo) | "punto" (internacional).</param>
    public static bool TryParse(string? raw, string mode, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var s = raw.Trim()
                   .Replace("€", "").Replace("$", "").Replace("£", "")
                   .Replace(" ", "").Replace(" ", "");
        s = s.Replace("EUR", "", StringComparison.OrdinalIgnoreCase)
             .Replace("USD", "", StringComparison.OrdinalIgnoreCase)
             .Trim();

        if (string.IsNullOrEmpty(s)) return false;

        // Negativo por signo o por notación contable con paréntesis
        var negative = s.StartsWith('-') || (s.StartsWith('(') && s.EndsWith(')'));
        s = s.Replace("(", "").Replace(")", "").Replace("+", "");
        if (s.StartsWith('-')) s = s[1..];

        var normalized = mode switch
        {
            "coma"  => s.Replace(".", "").Replace(",", "."),   // europeo forzado
            "punto" => s.Replace(",", ""),                     // internacional forzado
            _       => NormalizeAuto(s)
        };

        if (!decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return false;

        if (negative) value = -value;
        return true;
    }

    private static string NormalizeAuto(string s)
    {
        var lastComma = s.LastIndexOf(',');
        var lastDot = s.LastIndexOf('.');

        if (lastComma >= 0 && lastDot >= 0)
        {
            // El separador decimal es el que aparece más a la derecha
            return lastComma > lastDot
                ? s.Replace(".", "").Replace(",", ".")   // europeo:       1.234,56
                : s.Replace(",", "");                     // internacional: 1,234.56
        }

        if (lastComma >= 0)
            return s.Replace(",", ".");                    // solo coma => decimal

        return s;                                          // solo punto (o entero) => ya es válido
    }
}
