using AhorroLand.Shared.Application.Servicies; // Tu interfaz
using Microsoft.AspNetCore.Hosting; // ✅ Para IWebHostEnvironment
using Microsoft.AspNetCore.Http;    // ✅ Para IHttpContextAccessor

namespace AhorroLand.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string containerName)
    {
        // 1. Determinar la ruta física (wwwroot)
        // En Docker, esto mapeará a tu carpeta del VPS gracias al volumen
        string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        // 2. Crear la carpeta del contenedor (ej: "avatars") si no existe
        string folderPath = Path.Combine(webRootPath, containerName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 3. Generar un nombre único para evitar colisiones y caracteres raros
        string extension = Path.GetExtension(fileName);
        string newFileName = $"{Guid.NewGuid()}{extension}";
        string fullPath = Path.Combine(folderPath, newFileName);

        // 4. Escribir el archivo en disco
        // Nos aseguramos de rebobinar el stream por si acaso se leyó antes
        if (fileStream.CanSeek) fileStream.Position = 0;

        using (var fs = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        // 5. Generar la URL pública (ej: https://api.ahorroland.com/avatars/guid.jpg)
        // Usamos IHttpContextAccessor para saber el dominio actual automáticamente
        var currentUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";

        // Combinamos URL base + contenedor + nombre archivo
        // Usamos Path.AltDirectorySeparatorChar ('/') para asegurar formato URL válido en Windows/Linux
        var pathForDb = Path.Combine(currentUrl, containerName, newFileName).Replace("\\", "/");

        return pathForDb;
    }

    public Task DeleteFileAsync(string? fileRoute, string containerName)
    {
        if (string.IsNullOrEmpty(fileRoute))
        {
            return Task.CompletedTask;
        }

        // 1. Extraer solo el nombre del archivo de la URL completa
        // fileRoute es: https://dominio.com/avatars/imagen.jpg -> queremos "imagen.jpg"
        var fileName = Path.GetFileName(fileRoute);

        // 2. Construir ruta física
        string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string filePath = Path.Combine(webRootPath, containerName, fileName);

        // 3. Borrar si existe
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}