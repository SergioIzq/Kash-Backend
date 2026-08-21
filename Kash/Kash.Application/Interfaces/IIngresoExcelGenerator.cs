using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Renderiza un listado de Ingresos como un libro Excel de detalle (una fila por Ingreso).
/// </summary>
public interface IIngresoExcelGenerator
{
    /// <summary>Genera el Excel (.xlsx) y devuelve su contenido en memoria.</summary>
    byte[] Generar(IReadOnlyList<IngresoDto> datos);
}
