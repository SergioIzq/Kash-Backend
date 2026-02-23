using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.GastosProgramados.Commands;

/// <summary>
/// Comando para actualizar un gasto programado con soporte de auto-creación de entidades relacionadas.
/// Si se proporciona un nombre y el ID no existe, se creará la entidad automáticamente.
/// </summary>
public sealed record UpdateGastoProgramadoCommand : AbsUpdateCommand<GastoProgramado, GastoProgramadoId, GastoProgramadoDto>
{
    public required decimal Importe { get; init; }
    public required string Frecuencia { get; init; }
    public required DateTime? FechaEjecucion { get; init; }
    public string? Descripcion { get; init; }
    public bool Activo { get; init; } = true;

    // IDs de entidades relacionadas
    public required Guid CategoriaId { get; init; }
    public required Guid ConceptoId { get; init; }
    public Guid? ProveedorId { get; init; }   // 🔥 Opcional
    public Guid? PersonaId { get; init; }     // 🔥 Opcional
    public required Guid CuentaId { get; init; }
    public required Guid FormaPagoId { get; init; }

    // 🔥 Nombres opcionales para auto-creación
    public string? CategoriaNombre { get; init; }
    public string? ConceptoNombre { get; init; }
    public string? ProveedorNombre { get; init; }
    public string? PersonaNombre { get; init; }
    public string? CuentaNombre { get; init; }
    public string? FormaPagoNombre { get; init; }
}

