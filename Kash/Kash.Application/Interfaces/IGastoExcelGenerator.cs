using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Renderiza un listado de Gastos como un libro Excel de detalle (una fila por Gasto).
/// </summary>
public interface IGastoExcelGenerator
{
    /// <summary>Genera el Excel (.xlsx) y devuelve su contenido en memoria.</summary>
    byte[] Generar(IReadOnlyList<GastoDto> datos);
}
