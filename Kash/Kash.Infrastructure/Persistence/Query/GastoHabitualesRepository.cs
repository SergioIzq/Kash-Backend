using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class GastoHabitualesRepository : ApplicationInterface.IGastoHabitualesRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GastoHabitualesRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IReadOnlyList<GastoHabitualDto>> GetHabitualesAsync(Guid usuarioId, int limit, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = @"
            SELECT
                g.id_concepto AS ConceptoId,
                COALESCE(c.nombre, '') AS ConceptoNombre,
                c.id_categoria AS CategoriaId,
                cat.nombre AS CategoriaNombre,
                g.id_cuenta AS CuentaId,
                COALESCE(cta.nombre, '') AS CuentaNombre,
                g.id_forma_pago AS FormaPagoId,
                COALESCE(fp.nombre, '') AS FormaPagoNombre,
                g.id_proveedor AS ProveedorId,
                prov.nombre AS ProveedorNombre,
                g.id_persona AS PersonaId,
                p.nombre AS PersonaNombre,
                COUNT(*) AS Veces,
                MAX(g.fecha) AS UltimoUso
            FROM gastos g
            LEFT JOIN conceptos c ON g.id_concepto = c.id
            LEFT JOIN categorias cat ON c.id_categoria = cat.id
            LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
            LEFT JOIN formas_pago fp ON g.id_forma_pago = fp.id
            LEFT JOIN proveedores prov ON g.id_proveedor = prov.id
            LEFT JOIN personas p ON g.id_persona = p.id
            WHERE g.id_usuario = @UsuarioId
            GROUP BY
                g.id_concepto, c.nombre, c.id_categoria, cat.nombre,
                g.id_cuenta, cta.nombre, g.id_forma_pago, fp.nombre,
                g.id_proveedor, prov.nombre, g.id_persona, p.nombre
            HAVING COUNT(*) > 1
            ORDER BY Veces DESC, UltimoUso DESC
            LIMIT @Limit";

        var resultados = await connection.QueryAsync<GastoHabitualDto>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId, Limit = limit }, cancellationToken: cancellationToken));

        return resultados.ToList();
    }
}
