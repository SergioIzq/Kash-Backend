namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representación del proveedor para ser enviada fuera de la capa de aplicación.
    /// </summary>
    public record ProveedorDto
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public Guid UsuarioId { get; init; }
        public DateTime FechaCreacion { get; init; }
    }
}
