using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos.Reportes;

namespace Kash.Application.Features.Gastos.Queries.GetExcel;

/// <summary>
/// Genera el Excel con los Gastos del usuario que cumplen los filtros indicados
/// (todos opcionales y combinables), sin paginar.
/// </summary>
public sealed record GetGastosExcelQuery(
    Guid UsuarioId,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    string? SearchTerm,
    Guid[]? ConceptoIds,
    Guid[]? CategoriaIds,
    Guid[]? ProveedorIds,
    Guid[]? PersonaIds
) : IQuery<PresupuestoArchivoDto>;
