using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain
{
    public interface IPersonaWriteRepository : IWriteRepository<Persona, PersonaId>
    {
        Task<Result> CreateAsyncWithValidation(Persona entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Persona entity, CancellationToken cancellationToken = default);
    }
}
