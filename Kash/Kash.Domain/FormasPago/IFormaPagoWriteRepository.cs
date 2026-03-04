using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain;

public interface IFormaPagoWriteRepository : IWriteRepository<FormaPago, FormaPagoId>
{
    Task<Result<FormaPago>> FindOrCreateAsync(FormaPago entity, CancellationToken cancellationToken = default);
    Task<Result> CreateAsyncWithValidation(FormaPago entity, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(FormaPago entity, CancellationToken cancellationToken = default);
}