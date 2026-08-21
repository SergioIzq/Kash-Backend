using Kash.Shared.Application.Dtos;
using MediatR;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Gastos.Queries.Habituales;

public sealed record GetHabitualesGastosQuery(int Limit) : IRequest<Result<IReadOnlyList<GastoHabitualDto>>>
{
    public Guid UsuarioId { get; init; }
}
