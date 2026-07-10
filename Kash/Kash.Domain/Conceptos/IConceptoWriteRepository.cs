using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain;

public interface IConceptoWriteRepository : IWriteRepository<Concepto, ConceptoId>
{
    Task<Result> CreateAsyncWithValidation(Concepto entity, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Concepto entity, CancellationToken cancellationToken = default);
}