using Dapper;
using Kash.Application.Features.Movimientos.Commands.Import;
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

    public async Task<HashSet<string>> CargarClavesExistentesAsync(
        Guid usuarioId, string? cuentaNombre, DateTime desde, DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT 'Gasto' AS Tipo, DATE(g.fecha) AS Fecha, g.importe AS Importe, g.descripcion AS Descripcion
            FROM gastos g
            LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
            WHERE g.id_usuario = @UsuarioId
              AND DATE(g.fecha) BETWEEN @Desde AND @Hasta
              AND (@CuentaNombre IS NULL OR cta.nombre = @CuentaNombre)
            UNION ALL
            SELECT 'Ingreso' AS Tipo, DATE(i.fecha) AS Fecha, i.importe AS Importe, i.descripcion AS Descripcion
            FROM ingresos i
            LEFT JOIN cuentas cta ON i.id_cuenta = cta.id
            WHERE i.id_usuario = @UsuarioId
              AND DATE(i.fecha) BETWEEN @Desde AND @Hasta
              AND (@CuentaNombre IS NULL OR cta.nombre = @CuentaNombre)";

        var filas = await connection.QueryAsync<ClaveRow>(
            new CommandDefinition(sql,
                new
                {
                    UsuarioId = usuarioId,
                    Desde = desde.Date,
                    Hasta = hasta.Date,
                    CuentaNombre = string.IsNullOrWhiteSpace(cuentaNombre) ? null : cuentaNombre
                },
                cancellationToken: cancellationToken));

        var claves = new HashSet<string>();
        foreach (var fila in filas)
        {
            claves.Add(MovimientoDedupKey.Construir(
                fila.Tipo == "Gasto", fila.Fecha, fila.Importe, fila.Descripcion));
        }

        return claves;
    }

    private sealed record ClaveRow
    {
        public string Tipo { get; init; } = string.Empty;
        public DateTime Fecha { get; init; }
        public decimal Importe { get; init; }
        public string? Descripcion { get; init; }
    }
}
