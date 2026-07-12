using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Traspasos.Commands;

public sealed record DeleteTraspasoCommand(Guid Id) : AbsDeleteCommand<Traspaso, TraspasoId>(Id);
