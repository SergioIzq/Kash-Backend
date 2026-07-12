using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Cuentas.Queries;

/// <summary>
/// Representa la solicitud para crear un nuevo Cuenta.
/// </summary>
// Hereda de AbsCreateCommand<Entidad, DTO de Respuesta>
public sealed record GetCuentaByIdQuery(Guid Id) : AbsGetByIdQuery<Cuenta, CuentaId, CuentaDto>(Id)
{
}
