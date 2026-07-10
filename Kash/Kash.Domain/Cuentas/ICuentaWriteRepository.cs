using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain;

public interface ICuentaWriteRepository : IWriteRepository<Cuenta, CuentaId>
{
    Task<Result<Cuenta>> FindOrCreateAsync(Cuenta entity, CancellationToken cancellationToken = default);
    Task<Result> CreateAsyncWithValidation(Cuenta entity, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Cuenta entity, CancellationToken cancellationToken = default);
}