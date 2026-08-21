using Kash.Shared.Application.Dtos;
using MediatR;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Ingresos.Queries.Sugerencia;

public sealed record GetSugerenciaIngresoQuery(Guid ConceptoId) : IRequest<Result<IReadOnlyList<IngresoDto>>>
{
    public Guid UsuarioId { get; init; }
}
