namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representación de la Cuenta para ser enviada fuera de la capa de aplicación.
    /// </summary>
    public record CuentaDto
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public decimal Saldo { get; init; }
        public Guid UsuarioId { get; init; }
    }
}
