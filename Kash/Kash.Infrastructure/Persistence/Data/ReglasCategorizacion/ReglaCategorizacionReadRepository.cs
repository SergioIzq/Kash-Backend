using Dapper;
using Kash.Application.Interfaces.Repositories;
using Kash.Domain;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.ReglasCategorizacion
{
    public class ReglaCategorizacionReadRepository
        : AbsReadRepository<ReglaCategorizacion, ReglaCategorizacionDto, ReglaCategorizacionId>, IReglaCategorizacionReadRepository
    {
        public ReglaCategorizacionReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.Simple(
                tableName: "reglas_categorizacion",
                selectColumns: new List<string>
                {
                    "id as Id",
                    "patron as Patron",
                    "tipo as Tipo",
                    "categoria_nombre as CategoriaNombre",
                    "concepto_nombre as ConceptoNombre",
                    "proveedor_nombre as ProveedorNombre",
                    "forma_pago_nombre as FormaPagoNombre",
                    "prioridad as Prioridad",
                    "activo as Activo",
                    "id_usuario as UsuarioId",
                    "fecha_creacion as FechaCreacion"
                },
                searchableColumns: new List<string>
                {
                    "patron",
                    "categoria_nombre",
                    "concepto_nombre"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Patron", "patron" },
                    { "Prioridad", "prioridad" },
                    { "CategoriaNombre", "categoria_nombre" },
                    { "FechaCreacion", "fecha_creacion" }
                },
                defaultOrderBy: "prioridad ASC, fecha_creacion ASC"
            );
        }

        public async Task<IEnumerable<ReglaCategorizacionDto>> GetActivasOrdenadasAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
SELECT
    id                as Id,
    patron            as Patron,
    tipo              as Tipo,
    categoria_nombre  as CategoriaNombre,
    concepto_nombre   as ConceptoNombre,
    proveedor_nombre  as ProveedorNombre,
    forma_pago_nombre as FormaPagoNombre,
    prioridad         as Prioridad,
    activo            as Activo,
    id_usuario        as UsuarioId,
    fecha_creacion    as FechaCreacion
FROM reglas_categorizacion
WHERE id_usuario = @UsuarioId AND activo = 1
ORDER BY prioridad ASC, fecha_creacion ASC";

            return await connection.QueryAsync<ReglaCategorizacionDto>(
                new CommandDefinition(sql, new { UsuarioId = usuarioId }, cancellationToken: cancellationToken));
        }
    }
}
