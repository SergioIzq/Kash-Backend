using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Inversiones.Commands;

public sealed record DeleteInversionCommand(Guid Id)
    : AbsDeleteCommand<Inversion, InversionId>(Id);
