namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representación del Gasto para ser expuesta. Contiene IDs y datos clave de las entidades relacionadas.
    /// </summary>
    public record GastoDto
    {
        public Guid Id { get; init; }
        public decimal Importe { get; init; }
        public DateTime Fecha { get; init; }
        public string? Descripcion { get; init; }

        // Relaciones (Flattened)
        public Guid ConceptoId { get; init; }
        public string ConceptoNombre { get; init; } = string.Empty;
        public Guid? CategoriaId { get; init; } // NULLABLE: CategoriaId viene del Concepto (LEFT JOIN)
        public string? CategoriaNombre { get; init; } // NULLABLE: puede ser null si no hay categoría

        public Guid ProveedorId { get; init; }
        public string ProveedorNombre { get; init; } = string.Empty;

        public Guid? PersonaId { get; init; }
        public string PersonaNombre { get; init; } = string.Empty;

        public Guid CuentaId { get; init; }
        public string CuentaNombre { get; init; } = string.Empty;

        public Guid FormaPagoId { get; init; }
        public string FormaPagoNombre { get; init; } = string.Empty;

        public Guid UsuarioId { get; init; }
    }
}
