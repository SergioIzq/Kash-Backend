namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representación del cliente para ser enviada fuera de la capa de aplicación.
    /// </summary>
    public record ClienteDto
    {
        public Guid Id { get; init; }
        public required string Nombre { get; init; }
        public Guid UsuarioId { get; init; }
        public DateTime FechaCreacion { get; init; }
    }
}
