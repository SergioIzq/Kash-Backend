namespace AhorroLand.Shared.Application.Dtos
{
    /// <summary>
    /// Representación de un Traspaso para ser expuesta. Contiene los IDs de las cuentas y los valores primitivos.
    /// </summary>
    public record TraspasoDto
    {
        public Guid Id { get; init; }

        // Value Objects
        public decimal Importe { get; init; }
        public DateTime Fecha { get; init; }
        public string? Descripcion { get; init; }

        // Relaciones (Flattened)
        public bool Activo { get; init; }
        public Guid CuentaOrigenId { get; init; }
        public string CuentaOrigenNombre { get; init; } = string.Empty;
        public Guid CuentaDestinoId { get; init; }
        public string CuentaDestinoNombre { get; init; } = string.Empty;
        public Guid UsuarioId { get; init; }
    }
}