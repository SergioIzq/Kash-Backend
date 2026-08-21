using System.Globalization;
using Kash.Application.Interfaces;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Dtos.Reportes;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Features.Gastos.Queries.GetExcel;

/// <summary>
/// Obtiene los Gastos del usuario que cumplen los filtros indicados y los renderiza como Excel.
/// </summary>
public sealed class GetGastosExcelQueryHandler : IQueryHandler<GetGastosExcelQuery, PresupuestoArchivoDto>
{
    private readonly IGastoExportRepository _exportRepository;
    private readonly IGastoExcelGenerator _excelGenerator;

    public GetGastosExcelQueryHandler(
        IGastoExportRepository exportRepository,
        IGastoExcelGenerator excelGenerator)
    {
        _exportRepository = exportRepository;
        _excelGenerator = excelGenerator;
    }

    public async Task<Result<PresupuestoArchivoDto>> Handle(
        GetGastosExcelQuery request,
        CancellationToken cancellationToken)
    {
        if (request.UsuarioId == Guid.Empty)
        {
            return Result.Failure<PresupuestoArchivoDto>(
                Error.Validation("El ID del usuario no puede estar vacío."));
        }

        if (request.FechaInicio.HasValue && request.FechaFin.HasValue && request.FechaInicio > request.FechaFin)
        {
            return Result.Failure<PresupuestoArchivoDto>(
                Error.Validation("La fecha de inicio no puede ser posterior a la fecha de fin."));
        }

        var filtro = new GastoExportFiltro(
            request.FechaInicio,
            request.FechaFin,
            request.SearchTerm,
            request.ConceptoIds,
            request.CategoriaIds,
            request.ProveedorIds,
            request.PersonaIds);

        var datos = await _exportRepository.GetForExportAsync(request.UsuarioId, filtro, cancellationToken);

        var excel = _excelGenerator.Generar(datos);

        var nombre = string.Format(
            CultureInfo.InvariantCulture,
            "gastos_{0:yyyyMMdd}.xlsx",
            DateTime.UtcNow);

        return Result.Success(new PresupuestoArchivoDto(nombre, excel));
    }
}
