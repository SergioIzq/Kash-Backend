namespace AhorroLand.Shared.Application.Dtos
{
    /// <summary>
    /// Representación de la categoría para ser enviada fuera de la capa de aplicación.
    /// </summary>
    public record CategoriaDto(
            Guid Id,
            string Nombre,
            string? Descripcion,
            Guid UsuarioId,
            DateTime FechaCreacion
        );
}
