using Kash.Shared.Application.Dtos;
using MediatR;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Gastos.Queries.Sugerencia;

public sealed record GetSugerenciaGastoQuery(Guid ConceptoId) : IRequest<Result<IReadOnlyList<GastoDto>>>
{
    public Guid UsuarioId { get; init; }
}
