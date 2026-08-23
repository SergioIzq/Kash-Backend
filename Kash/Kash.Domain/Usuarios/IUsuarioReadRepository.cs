using Kash.Shared.Domain.ValueObjects;

namespace Kash.Domain;

public interface IUsuarioReadRepository
{
    Task<Usuario?> GetByEmailAsync(Email correo, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByConfirmationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByApiTokenHashAsync(string apiTokenHash, CancellationToken cancellationToken = default);
}
