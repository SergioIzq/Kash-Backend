namespace Kash.Shared.Application.Dtos
{
    /// <summary>
    /// Representación del Concepto, incluyendo la información esencial de su categoría.
    /// </summary>
    public record ConceptoDto
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public Guid CategoriaId { get; init; }
        public string CategoriaNombre { get; init; } = string.Empty;
        public Guid UsuarioId { get; init; }
    }
}
