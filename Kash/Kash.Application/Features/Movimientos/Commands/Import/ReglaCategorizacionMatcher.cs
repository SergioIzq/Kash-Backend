using Kash.Shared.Application.Dtos;

namespace Kash.Application.Features.Movimientos.Commands.Import;

/// <summary>
/// Aplica las reglas de auto-categorización del usuario a una descripción de movimiento.
/// Las reglas ya deben venir activas y ordenadas por prioridad (ascendente) desde el repositorio;
/// se devuelve la primera que matchea (gana la de mayor prioridad = número más bajo).
/// </summary>
internal static class ReglaCategorizacionMatcher
{
    public static ReglaCategorizacionDto? Encontrar(
        IEnumerable<ReglaCategorizacionDto> reglasActivasOrdenadas,
        string descripcion,
        string tipo)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return null;
        }

        foreach (var regla in reglasActivasOrdenadas)
        {
            if (!string.IsNullOrEmpty(regla.Tipo) && !regla.Tipo.Equals(tipo, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var patrones = regla.Patron.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (patrones.Any(p => descripcion.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                return regla;
            }
        }

        return null;
    }
}
