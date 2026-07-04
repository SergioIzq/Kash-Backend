namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representación de la regla de categorización para ser enviada fuera de la capa de aplicación.
    /// </summary>
    public record ReglaCategorizacionDto
    {
        public Guid Id { get; init; }
        public string Patron { get; init; } = string.Empty;
        public string? Tipo { get; init; }
        public string CategoriaNombre { get; init; } = string.Empty;
        public string? ConceptoNombre { get; init; }
        public string? ProveedorNombre { get; init; }
        public string? FormaPagoNombre { get; init; }
        public int Prioridad { get; init; }
        public bool Activo { get; init; }
        public Guid UsuarioId { get; init; }
        public DateTime FechaCreacion { get; init; }
    }
}
