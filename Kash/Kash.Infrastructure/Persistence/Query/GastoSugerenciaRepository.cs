using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class GastoSugerenciaRepository : ApplicationInterface.IGastoSugerenciaRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GastoSugerenciaRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GastoDto?> GetUltimoUsoAsync(Guid usuarioId, Guid conceptoId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // Mismas columnas/joins que GastoReadRepository.ConfigureRepository(), pero con orden
        // explícito fecha DESC, fecha_creacion DESC: "el más reciente" desempatando por creación
        // cuando varios gastos comparten la misma fecha de transacción (ver IGastoSugerenciaRepository).
        const string sql = @"
            SELECT
                g.id AS Id,
                g.importe AS Importe,
                g.fecha AS Fecha,
                g.descripcion AS Descripcion,
                g.id_concepto AS ConceptoId,
                COALESCE(c.nombre, '') AS ConceptoNombre,
                c.id_categoria AS CategoriaId,
                cat.nombre AS CategoriaNombre,
                g.id_proveedor AS ProveedorId,
                prov.nombre AS ProveedorNombre,
                g.id_persona AS PersonaId,
                p.nombre AS PersonaNombre,
                g.id_cuenta AS CuentaId,
                COALESCE(cta.nombre, '') AS CuentaNombre,
                g.id_forma_pago AS FormaPagoId,
                COALESCE(fp.nombre, '') AS FormaPagoNombre,
                g.id_usuario AS UsuarioId
            FROM gastos g
            LEFT JOIN conceptos c ON g.id_concepto = c.id
            LEFT JOIN categorias cat ON c.id_categoria = cat.id
            LEFT JOIN proveedores prov ON g.id_proveedor = prov.id
            LEFT JOIN personas p ON g.id_persona = p.id
            LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
            LEFT JOIN formas_pago fp ON g.id_forma_pago = fp.id
            WHERE g.id_usuario = @UsuarioId AND g.id_concepto = @ConceptoId
            ORDER BY g.fecha DESC, g.fecha_creacion DESC
            LIMIT 1";

        return await connection.QueryFirstOrDefaultAsync<GastoDto?>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId, ConceptoId = conceptoId }, cancellationToken: cancellationToken));
    }
}
