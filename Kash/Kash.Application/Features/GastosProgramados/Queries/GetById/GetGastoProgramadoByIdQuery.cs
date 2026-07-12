using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.GastosProgramados.Queries;

public sealed record GetGastoProgramadoByIdQuery(Guid Id) : AbsGetByIdQuery<GastoProgramado, GastoProgramadoId, GastoProgramadoDto>(Id)
{
}
