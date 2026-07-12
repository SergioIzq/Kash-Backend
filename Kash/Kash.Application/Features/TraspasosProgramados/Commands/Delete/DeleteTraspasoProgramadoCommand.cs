using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.TraspasosProgramados.Commands;

public sealed record DeleteTraspasoProgramadoCommand(Guid Id) : AbsDeleteCommand<TraspasoProgramado, TraspasoProgramadoId>(Id);
