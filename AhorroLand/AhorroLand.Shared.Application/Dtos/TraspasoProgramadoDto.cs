namespace AhorroLand.Shared.Application.Dtos
{
    /// <summary>
    /// Representación de un Traspaso Programado para ser expuesta. Contiene los IDs de las cuentas y los valores primitivos.
    /// </summary>
    public record TraspasoProgramadoDto
    {
        public Guid Id { get; init; }

        // Value Objects
        public decimal Importe { get; init; }
        public DateTime FechaEjecucion { get; init; }
        public string? Descripcion { get; init; }

        // Relaciones (Flattened)
        public Guid CuentaOrigenId { get; init; }
        public string CuentaOrigenNombre { get; init; } = string.Empty;
        public Guid CuentaDestinoId { get; init; }
        public string CuentaDestinoNombre { get; init; } = string.Empty;
        public Guid UsuarioId { get; init; }
        
        // ⭐ PROPIEDADES FALTANTES AÑADIDAS
        public string Frecuencia { get; init; } = string.Empty;
        public bool Activo { get; init; }
        public string HangfireJobId { get; init; } = string.Empty;
    }
}