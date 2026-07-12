using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain;

public interface IClienteWriteRepository : IWriteRepository<Cliente, ClienteId>
{
    Task<Result> CreateAsyncWithValidation(Cliente entity, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Cliente entity, CancellationToken cancellationToken = default);
}
