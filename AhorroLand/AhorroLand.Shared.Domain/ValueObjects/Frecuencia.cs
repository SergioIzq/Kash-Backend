using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct Frecuencia
{
    public string Value { get; init; }

    private static readonly string[] AllowedFrequencies = new[] { "Diaria", "Semanal", "Mensual", "Anual" };

    [Obsolete("No usar directamente. Utiliza Frecuencia.Create() para validación o Frecuencia.CreateFromDatabase() desde infraestructura.", error: true)]
    public Frecuencia()
    {
        Value = string.Empty;
    }

    private Frecuencia(string value)
    {
        Value = value;
    }

    public static Result<Frecuencia> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !AllowedFrequencies.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<Frecuencia>(Error.Validation($"La frecuencia '{value}' no es válida. Debe ser una de: {string.Join(", ", AllowedFrequencies)}."));
        }

        return Result.Success(new Frecuencia(value));
    }

    public static Frecuencia CreateFromDatabase(string value) => new Frecuencia(value);
}