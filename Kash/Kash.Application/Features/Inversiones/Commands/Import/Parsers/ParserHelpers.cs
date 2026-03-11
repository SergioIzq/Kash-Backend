namespace Kash.Application.Features.Inversiones.Commands.Import.Parsers;

internal static class ParserHelpers
{
    internal static string InferirTipo(string isin, string nombre)
    {
        var n = nombre.ToUpperInvariant();
        var i = isin.ToUpperInvariant();

        // Hemos añadido S&P, NASDAQ, STOXX y AMUNDI/ISHARES/VANGUARD como gestoras típicas
        if (n.Contains("ETF") || n.Contains("UCITS") || n.Contains("INDEX") ||
            n.Contains("MSCI") || n.Contains("FTSE") || n.Contains("S&P") ||
            n.Contains("NASDAQ") || n.Contains("STOXX") || n.Contains("ISHARES") ||
            n.Contains("VANGUARD") || n.Contains("AMUNDI"))
            return "etf";

        if (n.Contains("FUND") || n.Contains("SICAV") || n.Contains("FCP") ||
            n.Contains("FONDS"))
            return "fondo";

        if (n.Contains("BOND") || n.Contains("BONO") || n.Contains("OBLIG") ||
            n.Contains("TREASURY"))
            return "bono";

        if (i.Contains("BTC") || i.Contains("ETH") || i.Contains("USDT") ||
            n.Contains("CRYPTO"))
            return "cripto";

        return "accion";
    }

    internal static string DetectDelimiter(string text)
    {
        var firstLine = text.Split('\n', 2)[0];
        return firstLine.Contains(';') ? ";" : ",";
    }
}