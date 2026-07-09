namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representación del Ingreso para ser expuesta. Contiene IDs y datos clave de las entidades relacionadas.
    /// </summary>
    public record IngresoDto
    {
        public Guid Id { get; init; }
        public decimal Importe { get; init; }
        public DateTime Fecha { get; init; }
        public string? Descripcion { get; init; }

        // Relaciones (Flattened)
        public Guid ConceptoId { get; init; }
        public string ConceptoNombre { get; init; } = string.Empty;
        public Guid? CategoriaId { get; init; }
        public string? CategoriaNombre { get; init; } // NULLABLE: puede ser null si no hay categoría

        public Guid? ClienteId { get; init; } // NULLABLE: el ingreso puede no tener cliente
        public string? ClienteNombre { get; init; } // NULLABLE

        public Guid? PersonaId { get; init; } // NULLABLE: el ingreso puede no tener persona
        public string? PersonaNombre { get; init; } // NULLABLE

        public Guid CuentaId { get; init; }
        public string CuentaNombre { get; init; } = string.Empty;

        public Guid FormaPagoId { get; init; }
        public string FormaPagoNombre { get; init; } = string.Empty;

        public Guid UsuarioId { get; init; }
    }
}
