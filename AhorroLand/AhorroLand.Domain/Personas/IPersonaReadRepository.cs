using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain
{
    // 🔥 Solo contiene métodos de validación personalizados
    public interface IPersonaReadRepository
    {
        Task<bool> ExistsWithSameNameAsync(Nombre nombre, UsuarioId usuarioId, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithSameNameExceptAsync(Nombre nombre, UsuarioId usuarioId, Guid excludeId, CancellationToken cancellationToken = default);
    }
}
