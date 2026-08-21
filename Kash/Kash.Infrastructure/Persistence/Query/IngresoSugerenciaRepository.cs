using Dapper;
using Kash.Shared.Application.Dtos;
using SergioIzq.Infrastructure.Kernel.Persistence;
using ApplicationInterface = Kash.Application.Interfaces;

namespace Kash.Infrastructure.Persistence.Query;

public sealed class IngresoSugerenciaRepository : ApplicationInterface.IIngresoSugerenciaRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public IngresoSugerenciaRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IngresoDto?> GetUltimoUsoAsync(Guid usuarioId, Guid conceptoId, CancellationToken cancellationToken = default)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // Mismas columnas/joins que IngresoReadRepository.ConfigureRepository(), pero con orden
        // explícito fecha DESC, fecha_creacion DESC (ver IGastoSugerenciaRepository).
        const string sql = @"
            SELECT
                i.id AS Id,
                i.importe AS Importe,
                i.fecha AS Fecha,
                i.descripcion AS Descripcion,
                i.id_concepto AS ConceptoId,
                COALESCE(c.nombre, '') AS ConceptoNombre,
                cat.id AS CategoriaId,
                cat.nombre AS CategoriaNombre,
                i.id_cliente AS ClienteId,
                COALESCE(cli.nombre, '') AS ClienteNombre,
                i.id_persona AS PersonaId,
                COALESCE(p.nombre, '') AS PersonaNombre,
                i.id_cuenta AS CuentaId,
                COALESCE(cta.nombre, '') AS CuentaNombre,
                i.id_forma_pago AS FormaPagoId,
                COALESCE(fp.nombre, '') AS FormaPagoNombre,
                i.id_usuario AS UsuarioId
            FROM ingresos i
            LEFT JOIN conceptos c ON i.id_concepto = c.id
            LEFT JOIN categorias cat ON c.id_categoria = cat.id
            LEFT JOIN clientes cli ON i.id_cliente = cli.id
            LEFT JOIN personas p ON i.id_persona = p.id
            LEFT JOIN cuentas cta ON i.id_cuenta = cta.id
            LEFT JOIN formas_pago fp ON i.id_forma_pago = fp.id
            WHERE i.id_usuario = @UsuarioId AND i.id_concepto = @ConceptoId
            ORDER BY i.fecha DESC, i.fecha_creacion DESC
            LIMIT 1";

        return await connection.QueryFirstOrDefaultAsync<IngresoDto?>(
            new CommandDefinition(sql, new { UsuarioId = usuarioId, ConceptoId = conceptoId }, cancellationToken: cancellationToken));
    }
}
