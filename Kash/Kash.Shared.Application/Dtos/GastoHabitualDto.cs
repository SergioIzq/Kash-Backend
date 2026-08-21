namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representa una combinación completa de campos de gasto (concepto, categoría, cuenta,
    /// forma de pago, proveedor, persona) que un usuario registra con frecuencia, junto con
    /// el número de veces registrada y la fecha de uso más reciente. A diferencia de
    /// <see cref="GastoDto"/> no representa una transacción individual, sino un agregado.
    /// </summary>
    public record GastoHabitualDto
    {
        public Guid ConceptoId { get; init; }
        public string ConceptoNombre { get; init; } = string.Empty;
        public Guid? CategoriaId { get; init; }
        public string? CategoriaNombre { get; init; }

        public Guid CuentaId { get; init; }
        public string CuentaNombre { get; init; } = string.Empty;

        public Guid FormaPagoId { get; init; }
        public string FormaPagoNombre { get; init; } = string.Empty;

        public Guid? ProveedorId { get; init; }
        public string? ProveedorNombre { get; init; }

        public Guid? PersonaId { get; init; }
        public string? PersonaNombre { get; init; }

        public int Veces { get; init; }
        public DateTime UltimoUso { get; init; }
    }
}
