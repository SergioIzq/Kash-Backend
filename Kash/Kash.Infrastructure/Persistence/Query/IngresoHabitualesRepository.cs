using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class IngresoHabitualesRepository : ApplicationInterface.IIngresoHabitualesRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public IngresoHabitualesRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IReadOnlyList<IngresoHabitualDto>> GetHabitualesAsync(Guid usuarioId, int limit, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                i.id_concepto AS ConceptoId,
                COALESCE(c.nombre, '') AS ConceptoNombre,
                cat.id AS CategoriaId,
                cat.nombre AS CategoriaNombre,
                i.id_cuenta AS CuentaId,
                COALESCE(cta.nombre, '') AS CuentaNombre,
                i.id_forma_pago AS FormaPagoId,
                COALESCE(fp.nombre, '') AS FormaPagoNombre,
                i.id_cliente AS ClienteId,
                cli.nombre AS ClienteNombre,
                i.id_persona AS PersonaId,
                p.nombre AS PersonaNombre,
                COUNT(*) AS Veces,
                MAX(i.fecha) AS UltimoUso
            FROM ingresos i
            LEFT JOIN conceptos c ON i.id_concepto = c.id
            LEFT JOIN categorias cat ON c.id_categoria = cat.id
            LEFT JOIN cuentas cta ON i.id_cuenta = cta.id
            LEFT JOIN formas_pago fp ON i.id_forma_pago = fp.id
            LEFT JOIN clientes cli ON i.id_cliente = cli.id
            LEFT JOIN personas p ON i.id_persona = p.id
            WHERE i.id_usuario = @UsuarioId
            GROUP BY
                i.id_concepto, c.nombre, cat.id, cat.nombre,
                i.id_cuenta, cta.nombre, i.id_forma_pago, fp.nombre,
                i.id_cliente, cli.nombre, i.id_persona, p.nombre
            HAVING COUNT(*) > 1
            ORDER BY Veces DESC, UltimoUso DESC
            LIMIT @Limit";

        var resultados = await connection.QueryAsync<IngresoHabitualDto>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId, Limit = limit }, cancellationToken: cancellationToken));

        return resultados.ToList();
    }
}
