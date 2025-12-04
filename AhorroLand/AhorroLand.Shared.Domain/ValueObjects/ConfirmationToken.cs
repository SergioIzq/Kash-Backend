using System.Security.Cryptography;
using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct ConfirmationToken
{
    public string Value { get; }

    private const int TokenLength = 32;

    [Obsolete("No usar directamente. Utiliza ConfirmationToken.Create() para validación, ConfirmationToken.GenerateNew() para generar, o ConfirmationToken.CreateFromDatabase() desde infraestructura.", error: true)]
    public ConfirmationToken()
    {
        Value = string.Empty;
    }

    private ConfirmationToken(string value)
    {
        Value = value;
    }

    public static Result<ConfirmationToken> Create(string value)
    {
        // 🔑 Regla de Negocio: El token debe tener la longitud esperada.
        if (string.IsNullOrWhiteSpace(value) || value.Length != TokenLength)
        {
            return Result.Failure<ConfirmationToken>(Error.Validation($"El token debe tener exactamente {TokenLength} caracteres."));
        }

        return Result.Success(new ConfirmationToken(value));
    }

    /// <summary>
    /// Método de fábrica para generar un token seguro y aleatorio.
    /// </summary>
    public static ConfirmationToken GenerateNew()
    {
        // Calculamos los bytes necesarios para obtener al menos 32 caracteres Base64
        // 24 bytes * 1.33 = 32 caracteres
        var bytes = new byte[24];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        // Convertimos a Base64 URL Safe
        var tokenRaw = Convert.ToBase64String(bytes)
            .Replace("=", "")
            .Replace("+", "-")
            .Replace("/", "_");

        // Aseguramos que tomamos exactamente 32 (aunque con 24 bytes suele dar 32 exactos)ññ
        var token = tokenRaw.Length > TokenLength
            ? tokenRaw.Substring(0, TokenLength)
            : tokenRaw;

        return new ConfirmationToken(token);
    }

    public static ConfirmationToken CreateFromDatabase(string value) => new ConfirmationToken(value);

    /// <summary>
    /// Compara el Token actual con una cadena de texto cruda.
    /// Incluye limpieza (Trim) y comparación segura.
    /// </summary>
    public bool Equals(string? other)
    {
        if (string.IsNullOrWhiteSpace(other))
            return false;

        return string.Equals(Value, other.Trim(), StringComparison.Ordinal);
    }
}