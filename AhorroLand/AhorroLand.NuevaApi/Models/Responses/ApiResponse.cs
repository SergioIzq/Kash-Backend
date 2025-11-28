namespace AhorroLand.NuevaApi.Models.Responses;

/// <summary>
/// Respuesta genérica de API que envuelve cualquier tipo de dato.
/// </summary>
/// <typeparam name="T">Tipo de dato que se devuelve</typeparam>
public sealed record ApiResponse<T>
{
    /// <summary>
    /// Los datos de la respuesta
    /// </summary>
    public T Data { get; init; }

    /// <summary>
    /// Mensaje descriptivo opcional
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Timestamp de la respuesta
    /// </summary>
    public DateTime Timestamp { get; init; }

    public ApiResponse(T data, string? message = null, bool success = true)
    {
        Data = data;
        Message = message;
        Success = success;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? message = null)
    => new(data, message, true);

    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static ApiResponse<T> Error(string message)
    => new(default!, message, false);
}
