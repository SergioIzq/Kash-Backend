using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos.Reportes;

namespace Kash.Application.Features.Ingresos.Queries.GetExcel;

/// <summary>
/// Genera el Excel con los Ingresos del usuario que cumplen los filtros indicados
/// (todos opcionales y combinables), sin paginar.
/// </summary>
public sealed record GetIngresosExcelQuery(
    Guid UsuarioId,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    string? SearchTerm,
    Guid[]? ConceptoIds,
    Guid[]? CategoriaIds,
    Guid[]? ClienteIds,
    Guid[]? PersonaIds
) : IQuery<PresupuestoArchivoDto>;
