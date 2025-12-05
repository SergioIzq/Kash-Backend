namespace AhorroLand.Shared.Domain.Helpers;

public static class CacheKeys
{
    // Usamos el formato "D" para el GUID (guiones, minúsculas) para asegurar consistencia
    // Ejemplo: "usuarios:68baf011-4997-e479-93e9-0050565acf02"
    public static string Usuario(Guid id) => $"usuarios:{id.ToString("D")}";
}