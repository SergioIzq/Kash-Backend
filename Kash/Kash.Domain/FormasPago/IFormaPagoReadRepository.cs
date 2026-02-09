using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain;

// 🔥 Solo contiene métodos de validación personalizados
public interface IFormaPagoReadRepository
{
    Task<bool> ExistsWithSameNameAsync(Nombre nombre, UsuarioId usuarioId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithSameNameExceptAsync(Nombre nombre, UsuarioId usuarioId, Guid excludeId, CancellationToken cancellationToken = default);
    Task<FormaPago?> GetByNameAsync(Nombre nombre, UsuarioId usuarioId, CancellationToken cancellationToken = default);
}