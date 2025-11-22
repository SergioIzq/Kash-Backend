using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain;

/// <summary>
/// Interfaz para el repositorio de lectura de Clientes.
/// </summary>
// 🔥 Solo contiene métodos de validación personalizados
public interface IClienteReadRepository
{
    /// <summary>
    /// Verifica si ya existe un cliente con el mismo nombre para un usuario.
    /// </summary>
    Task<bool> ExistsWithSameNameAsync(Nombre nombre, UsuarioId usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si ya existe un cliente con el mismo nombre para un usuario, excluyendo un cliente específico (para updates).
    /// </summary>
    Task<bool> ExistsWithSameNameExceptAsync(Nombre nombre, UsuarioId usuarioId, Guid excludeId, CancellationToken cancellationToken = default);
}