using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain
{
    public interface IPersonaWriteRepository : IWriteRepository<Persona, PersonaId>
    {
        Task<Result> CreateAsyncWithValidation(Persona entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Persona entity, CancellationToken cancellationToken = default);
    }
}
