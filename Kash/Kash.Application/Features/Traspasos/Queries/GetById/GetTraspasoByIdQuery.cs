using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Traspasos.Queries;

public sealed record GetTraspasoByIdQuery(Guid Id) : AbsGetByIdQuery<Traspaso, TraspasoId, TraspasoDto>(Id)
{
}
