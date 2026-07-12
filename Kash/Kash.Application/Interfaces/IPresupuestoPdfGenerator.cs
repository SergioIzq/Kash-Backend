using Kash.Shared.Application.Dtos.Reportes;

namespace Kash.Application.Interfaces;

/// <summary>
/// Renderiza un <see cref="PresupuestoReportDto"/> como un documento PDF de presupuesto formal.
/// </summary>
public interface IPresupuestoPdfGenerator
{
    /// <summary>Genera el PDF y devuelve su contenido en memoria.</summary>
    byte[] Generar(PresupuestoReportDto datos);
}
