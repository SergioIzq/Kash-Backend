using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Clientes.Commands;

/// <summary>
/// Representa la solicitud para eliminar un Cliente por su identificador.
/// </summary>
// Hereda de AbsDeleteCommand<Entidad>
public sealed record DeleteClienteCommand(Guid Id)
    : AbsDeleteCommand<Cliente, ClienteId>(Id);
