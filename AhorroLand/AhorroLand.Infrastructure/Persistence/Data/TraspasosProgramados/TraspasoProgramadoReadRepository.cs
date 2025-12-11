using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Microsoft.Extensions.Caching.Distributed;

namespace AhorroLand.Infrastructure.Persistence.Data.TraspasoProgramados
{
    public class TraspasoProgramadoReadRepository : AbsReadRepository<TraspasoProgramado, TraspasoProgramadoDto, TraspasoProgramadoId>, ITraspasoProgramadoReadRepository
    {
        public TraspasoProgramadoReadRepository(
            IDbConnectionFactory dbConnectionFactory,
            IDistributedCache? cache = null)
            : base(dbConnectionFactory, "traspasos_programados", cache)
        {
        }

        protected override string GetTableAlias() => "tp";

        protected override string BuildGetByIdQuery()
        {
            return @"
                SELECT 
                    tp.id as Id,
                    tp.importe as Importe,
                    tp.fecha_ejecucion as FechaEjecucion,
                    tp.id_cuenta_origen as CuentaOrigenId,
                    co.nombre as CuentaOrigenNombre,
                    tp.id_cuenta_destino as CuentaDestinoId,
                    cd.nombre as CuentaDestinoNombre,
                    tp.id_usuario as UsuarioId,
                    tp.frecuencia as Frecuencia,
                    tp.descripcion as Descripcion,
                    tp.activo as Activo,
                    tp.hangfire_job_id as HangfireJobId,
                    tp.fecha_creacion as FechaCreacion
                FROM traspasos_programados tp
                LEFT JOIN cuentas co ON tp.id_cuenta_origen = co.id
                LEFT JOIN cuentas cd ON tp.id_cuenta_destino = cd.id
                WHERE tp.id = @id";
        }

        protected override string BuildGetAllQuery()
        {
            return @"
                SELECT 
                    tp.id as Id,
                    tp.importe as Importe,
                    tp.fecha_ejecucion as FechaEjecucion,
                    tp.id_cuenta_origen as CuentaOrigenId,
                    co.nombre as CuentaOrigenNombre,
                    tp.id_cuenta_destino as CuentaDestinoId,
                    cd.nombre as CuentaDestinoNombre,
                    tp.id_usuario as UsuarioId,
                    tp.frecuencia as Frecuencia,
                    tp.descripcion as Descripcion,
                    tp.activo as Activo,
                    tp.hangfire_job_id as HangfireJobId,
                    tp.fecha_creacion as FechaCreacion
                FROM traspasos_programados tp
                LEFT JOIN cuentas co ON tp.id_cuenta_origen = co.id
                LEFT JOIN cuentas cd ON tp.id_cuenta_destino = cd.id";
        }

        protected override string BuildGetPagedQuery()
        {
            return BuildGetAllQuery();
        }

        protected override string GetDefaultOrderBy()
        {
            return "ORDER BY tp.fecha_ejecucion DESC, tp.fecha_creacion DESC";
        }

        protected override Dictionary<string, string> GetSortableColumns()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "FechaEjecucion", "tp.fecha_ejecucion" },
                { "FechaCreacion", "tp.fecha_creacion" },
                { "Importe", "tp.importe" },
                { "CuentaOrigen", "co.nombre" },
                { "CuentaDestino", "cd.nombre" },
                { "Frecuencia", "tp.frecuencia" },
                { "Activo", "tp.activo" }
            };
        }

        protected override List<string> GetSearchableColumns()
        {
            return new List<string>
            {
                "co.nombre",
                "cd.nombre",
                "tp.descripcion",
                "tp.frecuencia"
            };
        }

        protected override List<string> GetNumericSearchableColumns()
        {
            return new List<string>
            {
                "tp.importe"
            };
        }

        protected override List<string> GetDateSearchableColumns()
        {
            return new List<string>
            {
                "tp.fecha_ejecucion",
                "tp.fecha_creacion"
            };
        }
    }
}