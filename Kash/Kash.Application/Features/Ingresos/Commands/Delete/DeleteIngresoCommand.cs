using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Ingresos.Commands;

/// <summary>
/// Representa la solicitud para eliminar una Ingreso por su identificador.
/// </summary>
// Hereda de AbsDeleteCommand<Entidad>
public sealed record DeleteIngresoCommand(Guid Id)
    : AbsDeleteCommand<Ingreso, IngresoId>(Id);
