namespace AhorroLand.Shared.Application.Dtos
{
    /// <summary>
    /// Representación del Ingreso para ser expuesta. Contiene IDs y datos clave de las entidades relacionadas.
    /// </summary>
    public record IngresoDto(
        Guid Id,
        decimal Importe,
        DateTime Fecha,
        string? Descripcion,

        // Relaciones (Flattened)
        Guid ConceptoId,
        string ConceptoNombre,
        Guid CategoriaId,
        string? CategoriaNombre, // 🔥 NULLABLE: puede ser null si no hay categoría

        Guid? ClienteId, // 🔥 NULLABLE: el ingreso puede no tener cliente
        string? ClienteNombre, // 🔥 NULLABLE

        Guid? PersonaId, // 🔥 NULLABLE: el ingreso puede no tener persona
        string? PersonaNombre, // 🔥 NULLABLE

        Guid CuentaId,
        string CuentaNombre,

        Guid FormaPagoId,
        string FormaPagoNombre,
        Guid UsuarioId
    );
}