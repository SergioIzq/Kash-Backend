using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Commands;

/// <summary>
/// Representa la solicitud para eliminar una regla de categorización por su identificador.
/// </summary>
public sealed record DeleteReglaCategorizacionCommand(Guid Id)
    : AbsDeleteCommand<ReglaCategorizacion, ReglaCategorizacionId>(Id);
