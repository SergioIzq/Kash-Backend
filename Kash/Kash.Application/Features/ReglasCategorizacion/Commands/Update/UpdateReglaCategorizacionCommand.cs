using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Commands;

/// <summary>
/// Representa la solicitud para actualizar una regla de categorización existente.
/// </summary>
public sealed record UpdateReglaCategorizacionCommand
    : AbsUpdateCommand<ReglaCategorizacion, ReglaCategorizacionId, ReglaCategorizacionDto>
{
    public required string Patron { get; init; }
    public string? Tipo { get; init; }
    public required string CategoriaNombre { get; init; }
    public string? ConceptoNombre { get; init; }
    public string? ProveedorNombre { get; init; }
    public string? FormaPagoNombre { get; init; }
    public int Prioridad { get; init; }
    public bool Activo { get; init; }
}
