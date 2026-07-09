using Kash.Domain;
using Kash.Infrastructure.Persistence.Query;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;
using Microsoft.Extensions.Caching.Distributed;

namespace Kash.Infrastructure.Persistence.Data.TraspasoProgramados
{
    public class TraspasoProgramadoReadRepository : AbsReadRepository<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId>, ITraspasoProgramadoReadRepository
    {
        public TraspasoProgramadoReadRepository(
            IDbConnectionFactory dbConnectionFactory,
            IDistributedCache? cache = null)
            : base(dbConnectionFactory, cache)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.WithJoins(
                tableName: "traspasos_programados",
                tableAlias: "tp",
                selectColumns: new List<string>
                {
                    "tp.id as Id",
                    "tp.importe as Importe",
                    "tp.fecha_ejecucion as FechaEjecucion",
                    "tp.id_cuenta_origen as CuentaOrigenId",
                    "co.nombre as CuentaOrigenNombre",
                    "tp.id_cuenta_destino as CuentaDestinoId",
                    "cd.nombre as CuentaDestinoNombre",
                    "tp.id_usuario as UsuarioId",
                    "tp.frecuencia as Frecuencia",
                    "tp.descripcion as Descripcion",
                    "tp.activo as Activo",
                    "tp.hangfire_job_id as HangfireJobId",
                    "tp.fecha_creacion as FechaCreacion"
                },
                joinClause: @"LEFT JOIN cuentas co ON tp.id_cuenta_origen = co.id
LEFT JOIN cuentas cd ON tp.id_cuenta_destino = cd.id",
                searchableColumns: new List<string>
                {
                    "co.nombre",
                    "cd.nombre",
                    "tp.descripcion",
                    "tp.frecuencia"
                },
                numericSearchableColumns: new List<string>
                {
                    "tp.importe"
                },
                dateSearchableColumns: new List<string>
                {
                    "tp.fecha_ejecucion",
                    "tp.fecha_creacion"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "FechaEjecucion", "tp.fecha_ejecucion" },
                    { "FechaCreacion", "tp.fecha_creacion" },
                    { "Importe", "tp.importe" },
                    { "CuentaOrigen", "co.nombre" },
                    { "CuentaDestino", "cd.nombre" },
                    { "Frecuencia", "tp.frecuencia" },
                    { "Activo", "tp.activo" }
                },
                defaultOrderBy: "tp.fecha_ejecucion DESC, tp.fecha_creacion DESC"
            );
        }
    }
}
