using System.Globalization;
using Kash.Application.Interfaces;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos.Reportes;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Reportes.Queries.GetPresupuestoExcel;

/// <summary>
/// Obtiene los datos agregados del presupuesto y los renderiza como Excel.
/// </summary>
public sealed class GetPresupuestoExcelQueryHandler : IQueryHandler<GetPresupuestoExcelQuery, PresupuestoArchivoDto>
{
    private const int MaxDiasRango = 366;

    private readonly IReporteRepository _reporteRepository;
    private readonly IPresupuestoExcelGenerator _excelGenerator;

    public GetPresupuestoExcelQueryHandler(
        IReporteRepository reporteRepository,
        IPresupuestoExcelGenerator excelGenerator)
    {
        _reporteRepository = reporteRepository;
        _excelGenerator = excelGenerator;
    }

    public async Task<Result<PresupuestoArchivoDto>> Handle(
        GetPresupuestoExcelQuery request,
        CancellationToken cancellationToken)
    {
        if (request.UsuarioId == Guid.Empty)
        {
            return Result.Failure<PresupuestoArchivoDto>(
                Error.Validation("El ID del usuario no puede estar vacío."));
        }

        if (request.FechaInicio > request.FechaFin)
        {
            return Result.Failure<PresupuestoArchivoDto>(
                Error.Validation("La fecha de inicio no puede ser posterior a la fecha de fin."));
        }

        if ((request.FechaFin - request.FechaInicio).TotalDays > MaxDiasRango)
        {
            return Result.Failure<PresupuestoArchivoDto>(
                Error.Validation("El rango de fechas no puede exceder un año."));
        }

        // Normalizar el rango a día completo: [inicio 00:00, fin 23:59:59].
        var desde = request.FechaInicio.Date;
        var hasta = request.FechaFin.Date.AddDays(1).AddTicks(-1);

        var datos = await _reporteRepository.GetPresupuestoAsync(
            request.UsuarioId, desde, hasta, cancellationToken);

        var excel = _excelGenerator.Generar(datos);

        var nombre = string.Format(
            CultureInfo.InvariantCulture,
            "presupuesto_{0:yyyyMMdd}_{1:yyyyMMdd}.xlsx",
            desde,
            request.FechaFin.Date);

        return Result.Success(new PresupuestoArchivoDto(nombre, excel));
    }
}
