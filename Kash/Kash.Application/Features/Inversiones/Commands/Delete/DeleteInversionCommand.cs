using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Inversiones.Commands;

public sealed record DeleteInversionCommand(Guid Id)
    : AbsDeleteCommand<Inversion, InversionId>(Id);
