using Kash.Domain;
using Kash.Shared.Application.Dtos;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Interfaces.Repositories;

public interface IInversionReadRepository : IReadRepository<Inversion, InversionDto, InversionId>
{
    Task<bool> ExistsDuplicateAsync(
        Guid usuarioId,
        string ticker,
        DateOnly fechaCompra,
        decimal cantidad,
        decimal precioCompra,
        CancellationToken cancellationToken = default);
}
