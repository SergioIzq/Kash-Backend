using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.IngresosProgramados.Commands;

public sealed record DeleteIngresoProgramadoCommand(Guid Id) : AbsDeleteCommand<IngresoProgramado, IngresoProgramadoId>(Id);
