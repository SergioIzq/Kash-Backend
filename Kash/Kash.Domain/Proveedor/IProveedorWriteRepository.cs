using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain
{
    public interface IProveedorWriteRepository : IWriteRepository<Proveedor, ProveedorId>
    {
        Task<Result> CreateAsyncWithValidation(Proveedor entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Proveedor entity, CancellationToken cancellationToken = default);
    }
}