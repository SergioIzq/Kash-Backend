using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.TraspasosProgramados.Queries;

public sealed record GetTraspasoProgramadoByIdQuery(Guid Id) : AbsGetByIdQuery<TraspasoProgramado, TraspasoProgramadoId, TraspasoProgramadoDto>(Id)
{
}
