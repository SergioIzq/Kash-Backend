using Kash.Shared.Application.Dtos.Reportes;

namespace Kash.Application.Interfaces;

/// <summary>
/// Renderiza un <see cref="PresupuestoReportDto"/> como un libro Excel de presupuesto formateado.
/// </summary>
public interface IPresupuestoExcelGenerator
{
    /// <summary>Genera el Excel (.xlsx) y devuelve su contenido en memoria.</summary>
    byte[] Generar(PresupuestoReportDto datos);
}
