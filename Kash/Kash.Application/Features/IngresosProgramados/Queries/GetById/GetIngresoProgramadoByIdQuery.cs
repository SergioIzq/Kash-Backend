using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.IngresosProgramados.Queries;

public sealed record GetIngresoProgramadoByIdQuery(Guid Id) : AbsGetByIdQuery<IngresoProgramado, IngresoProgramadoId, IngresoProgramadoDto>(Id)
{
}
