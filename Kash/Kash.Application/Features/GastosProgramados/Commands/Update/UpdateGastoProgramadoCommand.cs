using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.GastosProgramados.Commands;

public sealed record UpdateGastoProgramadoCommand : AbsUpdateCommand<GastoProgramado, GastoProgramadoId, GastoProgramadoDto>
{
    public required decimal Importe { get; init; }
    public required string Frecuencia { get; init; }
    public required DateTime? FechaEjecucion { get; init; }
    public string? Descripcion { get; init; }
    public required Guid ConceptoId { get; init; }
    public required string ConceptoNombre { get; init; }
    public Guid ProveedorId { get; init; }
    public Guid CategoriaId { get; init; }
    public Guid PersonaId { get; init; }
    public required Guid CuentaId { get; init; }
    public required Guid FormaPagoId { get; init; }
    public bool Activo { get; init; } = true;
    public string? CategoriaNombre { get; init; } // 🔥 Para auto-creación
    public string? ProveedorNombre { get; init; } // 🔥 Para auto-creación
    public string? PersonaNombre { get; init; } // 🔥 Para auto-creación
    public string? CuentaNombre { get; init; } // 🔥 Para auto-creación
    public string? FormaPagoNombre { get; init; } // 🔥 Para auto-creación
}
