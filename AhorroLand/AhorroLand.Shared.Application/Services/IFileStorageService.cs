namespace AhorroLand.Shared.Application.Servicies;

public interface IFileStorageService
{
    /// <summary>
    /// Guarda un archivo y devuelve la URL/Ruta de acceso.
    /// </summary>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string containerName);

    /// <summary>
    /// Elimina un archivo existente (para limpiar cuando cambian de avatar).
    /// </summary>
    Task DeleteFileAsync(string? fileRoute, string containerName);
}