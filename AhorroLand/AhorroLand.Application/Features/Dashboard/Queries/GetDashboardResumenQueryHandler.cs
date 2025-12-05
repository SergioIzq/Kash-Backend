using AhorroLand.Application.Interfaces;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Dashboard.Queries;

/// <summary>
/// Handler para obtener el resumen del dashboard con filtros opcionales.
/// </summary>
public sealed class GetDashboardResumenQueryHandler : IQueryHandler<GetDashboardResumenQuery, DashboardResumenDto>
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardResumenQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<Result<DashboardResumenDto>> Handle(GetDashboardResumenQuery request, CancellationToken cancellationToken)
    {
        // Validaciones de entrada
        if (request.UsuarioId == Guid.Empty)
        {
            return Result.Failure<DashboardResumenDto>(
                Error.Validation("El ID del usuario no puede estar vacío."));
        }

        // Validar que las fechas sean lógicas
        if (request.FechaInicio.HasValue && request.FechaFin.HasValue)
        {
            if (request.FechaInicio.Value > request.FechaFin.Value)
            {
                return Result.Failure<DashboardResumenDto>(
                    Error.Validation("La fecha de inicio no puede ser posterior a la fecha de fin."));
            }

            // Validar que el rango de fechas no sea excesivo (ej: máximo 1 año)
            if ((request.FechaFin.Value - request.FechaInicio.Value).TotalDays > 365)
            {
                return Result.Failure<DashboardResumenDto>(
                    Error.Validation("El rango de fechas no puede exceder un año."));
            }
        }

        try
        {
            // Obtener el resumen completo del dashboard con filtros
            var resumen = await _dashboardRepository.GetDashboardResumenAsync(
                request.UsuarioId,
                request.FechaInicio,
                request.FechaFin,
                request.CuentaId,
                request.CategoriaId,
                cancellationToken);

            if (resumen == null)
            {
                return Result.Failure<DashboardResumenDto>(
                    Error.NotFound("No se pudo generar el resumen del dashboard para el usuario especificado."));
            }

            return Result.Success(resumen);
        }
        catch (Exception ex)
        {
            return Result.Failure<DashboardResumenDto>(
                Error.Failure("Dashboard.Error", "Error al obtener resumen", ex.Message));
        }
    }
}
