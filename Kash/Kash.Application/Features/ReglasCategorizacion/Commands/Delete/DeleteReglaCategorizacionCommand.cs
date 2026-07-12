using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Commands;

/// <summary>
/// Representa la solicitud para eliminar una regla de categorización por su identificador.
/// </summary>
public sealed record DeleteReglaCategorizacionCommand(Guid Id)
    : AbsDeleteCommand<ReglaCategorizacion, ReglaCategorizacionId>(Id);
