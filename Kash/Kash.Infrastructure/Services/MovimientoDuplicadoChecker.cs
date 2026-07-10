using Dapper;
using Kash.Application.Interfaces.Repositories;
using SergioIzq.Infrastructure.Kernel.Persistence;

namespace Kash.Infrastructure.Services;

/// <summary>
/// Implementación Dapper de <see cref="IMovimientoDuplicadoChecker"/>.
/// Se auto-registra vía Scrutor (namespace Kash.Infrastructure.Services).
/// </summary>
public sealed class MovimientoDuplicadoChecker : IMovimientoDuplicadoChecker
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovimientoDuplicadoChecker(IDbConnectionFactory dbConnectionFactory)
        => _dbConnectionFactory = dbConnectionFactory;

    public Task<bool> ExisteGastoAsync(
        Guid usuarioId, DateTime fecha, decimal importe, string? descripcion, string? cuentaNombre,
        CancellationToken cancellationToken = default)
        => ExisteAsync("gastos", "g", usuarioId, fecha, importe, descripcion, cuentaNombre, cancellationToken);

    public Task<bool> ExisteIngresoAsync(
        Guid usuarioId, DateTime fecha, decimal importe, string? descripcion, string? cuentaNombre,
        CancellationToken cancellationToken = default)
        => ExisteAsync("ingresos", "i", usuarioId, fecha, importe, descripcion, cuentaNombre, cancellationToken);

    private async Task<bool> ExisteAsync(
        string tabla, string alias,
        Guid usuarioId, DateTime fecha, decimal importe, string? descripcion, string? cuentaNombre,
        CancellationToken cancellationToken)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // 'tabla' y 'alias' son constantes del propio código (no entrada de usuario) => interpolación segura.
        var sql = $@"
                SELECT COUNT(1)
                FROM {tabla} {alias}
                LEFT JOIN cuentas cta ON {alias}.id_cuenta = cta.id
                WHERE {alias}.id_usuario = @UsuarioId
                  AND DATE({alias}.fecha) = @Fecha
                  AND ABS({alias}.importe - @Importe) < 0.005
                  AND (@Descripcion IS NULL OR {alias}.descripcion = @Descripcion)
                  AND (@CuentaNombre IS NULL OR cta.nombre = @CuentaNombre)
                LIMIT 1";

        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql,
                new
                {
                    UsuarioId = usuarioId,
                    Fecha = fecha.Date,
                    Importe = importe,
                    Descripcion = string.IsNullOrEmpty(descripcion) ? null : descripcion,
                    CuentaNombre = string.IsNullOrWhiteSpace(cuentaNombre) ? null : cuentaNombre
                },
                cancellationToken: cancellationToken));

        return count > 0;
    }
}
