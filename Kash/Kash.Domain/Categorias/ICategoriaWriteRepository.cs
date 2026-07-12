using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain
{
    public interface ICategoriaWriteRepository : IWriteRepository<Categoria, CategoriaId>
    {
        Task<Result<Categoria>> FindOrCreateAsync(Categoria entity, CancellationToken cancellationToken = default);
        Task<Result> CreateAsyncWithValidation(Categoria entity, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Categoria entity, CancellationToken cancellationToken = default);
    }
}
