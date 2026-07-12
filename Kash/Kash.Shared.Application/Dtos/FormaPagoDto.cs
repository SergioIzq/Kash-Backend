namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representación de la FormaPago para ser enviada fuera de la capa de aplicación.
    /// </summary>
    public record FormaPagoDto
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public Guid UsuarioId { get; init; }
    }
}
