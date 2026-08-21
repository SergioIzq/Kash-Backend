using Kash.Shared.Application.Dtos;

namespace Kash.Application.Interfaces;

/// <summary>
/// Último gasto registrado por un usuario para un concepto dado, usado para pre-rellenar un
/// alta nueva (capability sugerencias-transaccion). Vive en Application, no en Domain, mismo
/// motivo que <see cref="IGastoHabitualesRepository"/>: no es estrictamente necesario (GastoDto
/// no depende de Application desde Domain), pero se agrupa junto al resto de repositorios de
/// "sugerencia/habituales" por consistencia.
///
/// No reutiliza el `GetRecentAsync` genérico del kernel porque ese método ordena según el
/// `DefaultOrderBy` configurado en `GastoReadRepository` (`g.fecha DESC, g.id DESC`): con varias
/// gastos en la misma fecha, el desempate por `id` (GUID) no refleja cuál se creó realmente más
/// tarde. Aquí se ordena explícitamente por `fecha DESC, fecha_creacion DESC`.
/// </summary>
public interface IGastoSugerenciaRepository
{
    /// <summary>
    /// Último gasto del usuario para ese concepto (por fecha de transacción, desempatando por
    /// fecha de creación si coincide la fecha), o <c>null</c> si no hay histórico.
    /// </summary>
    Task<GastoDto?> GetUltimoUsoAsync(Guid usuarioId, Guid conceptoId, CancellationToken cancellationToken = default);
}
