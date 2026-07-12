using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.FormasPago.Commands;

/// <summary>
/// Representa la solicitud para eliminar una FormaPago por su identificador.
/// </summary>
// Hereda de AbsDeleteCommand<Entidad>
public sealed record DeleteFormaPagoCommand(Guid Id)
    : AbsDeleteCommand<FormaPago, FormaPagoId>(Id);
