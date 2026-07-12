using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Commands;

/// <summary>
/// Representa la solicitud para crear una nueva regla de categorización.
/// </summary>
public sealed record CreateReglaCategorizacionCommand : AbsCreateCommand<ReglaCategorizacion, ReglaCategorizacionId>
{
    public required string Patron { get; init; }
    public string? Tipo { get; init; }
    public required string CategoriaNombre { get; init; }
    public string? ConceptoNombre { get; init; }
    public string? ProveedorNombre { get; init; }
    public string? FormaPagoNombre { get; init; }
    public int Prioridad { get; init; } = 0;
    public bool Activo { get; init; } = true;
    public required Guid UsuarioId { get; init; }
}
