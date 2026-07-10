using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain;

public interface IFormaPagoWriteRepository : IWriteRepository<FormaPago, FormaPagoId>
{
    Task<Result<FormaPago>> FindOrCreateAsync(FormaPago entity, CancellationToken cancellationToken = default);
    Task<Result> CreateAsyncWithValidation(FormaPago entity, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(FormaPago entity, CancellationToken cancellationToken = default);
}