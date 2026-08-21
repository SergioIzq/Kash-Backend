using Kash.Shared.Application.Dtos;
using MediatR;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Ingresos.Queries.Habituales;

public sealed record GetHabitualesIngresosQuery(int Limit) : IRequest<Result<IReadOnlyList<IngresoHabitualDto>>>
{
    public Guid UsuarioId { get; init; }
}
