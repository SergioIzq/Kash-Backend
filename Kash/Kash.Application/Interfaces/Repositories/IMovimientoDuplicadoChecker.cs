namespace Kash.Application.Interfaces.Repositories;

/// <summary>
/// Comprueba en base de datos si un movimiento ya existe, para evitar duplicados
/// al reimportar extractos solapados. La clave es (usuario, fecha, importe, descripción, cuenta).
/// </summary>
public interface IMovimientoDuplicadoChecker
{
    Task<bool> ExisteGastoAsync(
        Guid usuarioId,
        DateTime fecha,
        decimal importe,
        string? descripcion,
        string? cuentaNombre,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteIngresoAsync(
        Guid usuarioId,
        DateTime fecha,
        decimal importe,
        string? descripcion,
        string? cuentaNombre,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Carga en una sola consulta las claves de todos los movimientos existentes del usuario en
    /// la cuenta y rango de fechas dados, para deduplicar una importación completa en memoria
    /// (evita una query por fila). El formato de cada clave es
    /// <c>{Tipo}|{yyyy-MM-dd}|{importe con 2 decimales, invariante}|{descripción}</c>,
    /// con Tipo = "Gasto" | "Ingreso" y descripción vacía si es nula.
    /// </summary>
    Task<HashSet<string>> CargarClavesExistentesAsync(
        Guid usuarioId,
        string? cuentaNombre,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);
}
