namespace Kash.Domain;

public enum TipoInversion
{
    Etf,
    Accion,
    Cripto,
    Bono,
    Fondo,
    MercadoPrivado,
    Otro
}

/// <summary>
/// Conversión entre TipoInversion y su representación en base de datos / JSON.
/// Valores: "etf" | "accion" | "cripto" | "bono" | "fondo" | "mercado_privado" | "otro"
/// </summary>
public static class TipoInversionConverter
{
    private static readonly Dictionary<TipoInversion, string> _toDb = new()
    {
        [TipoInversion.Etf]            = "etf",
        [TipoInversion.Accion]         = "accion",
        [TipoInversion.Cripto]         = "cripto",
        [TipoInversion.Bono]           = "bono",
        [TipoInversion.Fondo]          = "fondo",
        [TipoInversion.MercadoPrivado] = "mercado_privado",
        [TipoInversion.Otro]           = "otro"
    };

    private static readonly Dictionary<string, TipoInversion> _fromDb =
        _toDb.ToDictionary(kv => kv.Value, kv => kv.Key);

    public static string ToDb(TipoInversion tipo) => _toDb[tipo];

    public static TipoInversion FromDb(string value)
    {
        if (_fromDb.TryGetValue(value?.ToLowerInvariant() ?? "", out var tipo))
            return tipo;

        throw new ArgumentException($"Tipo de inversión inválido: '{value}'");
    }

    public static bool IsValid(string? value) =>
        value is not null && _fromDb.ContainsKey(value.ToLowerInvariant());

    public static IEnumerable<string> ValidValues => _toDb.Values;
}
