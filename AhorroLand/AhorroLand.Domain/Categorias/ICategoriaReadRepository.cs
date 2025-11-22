using AhorroLand.Shared.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AhorroLand.Domain
{
    // 🔥 Solo contiene métodos de validación personalizados
    // La implementación también hereda de IReadRepositoryWithDto
    public interface ICategoriaReadRepository
    {
        Task<bool> ExistsWithSameNameAsync(Nombre nombre, UsuarioId usuarioId, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithSameNameExceptAsync(Nombre nombre, UsuarioId usuarioId, Guid excludeId, CancellationToken cancellationToken = default);
    }
}