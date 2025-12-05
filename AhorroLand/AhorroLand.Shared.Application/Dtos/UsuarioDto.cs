namespace AhorroLand.Shared.Application.Dtos;

// Propiedades init-only (Dapper llena solo lo que encuentra)
public record UsuarioDto
{
    public Guid Id { get; init; }
    public string Correo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Apellidos { get; init; } = string.Empty;
    public string? Avatar { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }

    // Estas quedarán null automáticamente si el SQL no las trae
    public string? Rol { get; init; }
}