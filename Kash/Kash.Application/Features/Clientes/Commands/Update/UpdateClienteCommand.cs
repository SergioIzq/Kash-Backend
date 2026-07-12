using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Clientes.Commands;

/// <summary>
/// Representa la solicitud para actualizar una nueva Categoría.
/// </summary>
// Hereda de AbsUpadteCommand<Entidad, DTO de Respuesta>
public sealed record UpdateClienteCommand : AbsUpdateCommand<Cliente, ClienteId, ClienteDto>
{
    /// <summary>
    /// Nombre de la nueva categoría.
    /// </summary>
    public required string Nombre { get; init; }
}
