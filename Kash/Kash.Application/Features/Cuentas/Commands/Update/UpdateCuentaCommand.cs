using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Commands;

/// <summary>
/// Representa la solicitud para actualizar una nueva cuenta.
/// </summary>
// Hereda de AbsUpadteCommand<Entidad, DTO de Respuesta>
public sealed record UpdateCuentaCommand : AbsUpdateCommand<Cuenta, CuentaId, CuentaDto>
{
    /// <summary>
    /// Nombre de la nueva cuenta.
    /// </summary>
    public required string Nombre { get; init; }
}
