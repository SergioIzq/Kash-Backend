using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Proveedores.Commands;

/// <summary>
/// Representa la solicitud para actualizar un nuevo proveedor.
/// </summary>
// Hereda de AbsUpadteCommand<Entidad, DTO de Respuesta>
public sealed record UpdateProveedorCommand : AbsUpdateCommand<Proveedor, ProveedorId, ProveedorDto>
{
    /// <summary>
    /// Nombre del proveedor.
    /// </summary>
    public required string Nombre { get; init; }
}
