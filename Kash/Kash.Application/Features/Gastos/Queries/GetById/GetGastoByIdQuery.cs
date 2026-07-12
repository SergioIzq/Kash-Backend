using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Gastos.Queries;

/// <summary>
/// Representa la solicitud para crear un nuevo Gasto.
/// </summary>
// Hereda de AbsCreateCommand<Entidad, DTO de Respuesta>
public sealed record GetGastoByIdQuery(Guid Id) : AbsGetByIdQuery<Gasto, GastoId, GastoDto>(Id)
{
}
