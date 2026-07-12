using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.GastosProgramados.Commands;

public sealed record DeleteGastoProgramadoCommand(Guid Id) : AbsDeleteCommand<GastoProgramado, GastoProgramadoId>(Id);
