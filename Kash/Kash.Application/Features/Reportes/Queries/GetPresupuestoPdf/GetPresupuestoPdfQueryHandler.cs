using System.Globalization;
using Kash.Application.Interfaces;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos.Reportes;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Reportes.Queries.GetPresupuestoPdf;

/// <summary>
/// Obtiene los datos agregados del presupuesto y los renderiza como PDF.
/// </summary>
public sealed class GetPresupuestoPdfQueryHandler : IQueryHandler<GetPresupuestoPdfQuery, PresupuestoArchivoDto>
{
    private const int MaxDiasRango = 366;

    private readonly IReporteRepository _reporteRepository;
    private readonly IPresupuestoPdfGenerator _pdfGenerator;

    public GetPresupuestoPdfQueryHandler(
        IReporteRepository reporteRepository,
        IPresupuestoPdfGenerator pdfGenerator)
    {
        _reporteRepository = reporteRepository;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<Result<PresupuestoArchivoDto>> Handle(
        GetPresupuestoPdfQuery request,
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

        var pdf = _pdfGenerator.Generar(datos);

        var nombre = string.Format(
            CultureInfo.InvariantCulture,
            "presupuesto_{0:yyyyMMdd}_{1:yyyyMMdd}.pdf",
            desde,
            request.FechaFin.Date);

        return Result.Success(new PresupuestoArchivoDto(nombre, pdf));
    }
}
