namespace Kash.Shared.Application.Dtos
{
    public record GastoProgramadoDto
    {
        public Guid Id { get; init; }
        public decimal Importe { get; init; }
        public DateTime FechaEjecucion { get; init; }
        public string? Descripcion { get; init; }

        // PROPIEDADES FALTANTES AÑADIDAS
        public string Frecuencia { get; init; } = string.Empty;
        public bool Activo { get; init; }
        public string HangfireJobId { get; init; } = string.Empty;

        // Relaciones (Flattened)
        public Guid ConceptoId { get; init; }
        public string ConceptoNombre { get; init; } = string.Empty;
        public Guid CategoriaId { get; init; }
        public string CategoriaNombre { get; init; } = string.Empty;

        public Guid ProveedorId { get; init; }
        public string ProveedorNombre { get; init; } = string.Empty;

        public Guid PersonaId { get; init; }
        public string PersonaNombre { get; init; } = string.Empty;

        public Guid CuentaId { get; init; }
        public string CuentaNombre { get; init; } = string.Empty;

        public Guid FormaPagoId { get; init; }
        public string FormaPagoNombre { get; init; } = string.Empty;

        public Guid UsuarioId { get; init; }
    }
}
