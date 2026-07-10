using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Commands;

/// <summary>
/// Representa la solicitud para eliminar una Cuenta por su identificador.
/// </summary>
// Hereda de AbsDeleteCommand<Entidad>
public sealed record DeleteCuentaCommand(Guid Id)
    : AbsDeleteCommand<Cuenta, CuentaId>(Id);
