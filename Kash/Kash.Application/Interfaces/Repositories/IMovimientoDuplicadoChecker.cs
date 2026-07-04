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
}
