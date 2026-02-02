using Kash.Domain;
using Kash.Infrastructure.Persistence.Query;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.IngresosProgramados
{
    public class IngresoProgramadoReadRepository : AbsReadRepository<IngresoProgramado, IngresoProgramadoDto, IngresoProgramadoId>, IIngresoProgramadoReadRepository
    {
        public IngresoProgramadoReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// 🔥 ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.WithJoins(
                tableName: "ingresos_programados",
                tableAlias: "ip",
                selectColumns: new List<string>
                {
                    "ip.id as Id",
                    "ip.importe as Importe",
                    "ip.fecha_ejecucion as FechaEjecucion",
                    "ip.descripcion as Descripcion",
                    "ip.frecuencia as Frecuencia",
                    "ip.activo as Activo",
                    "ip.hangfire_job_id as HangfireJobId",
                    "ip.id_concepto as ConceptoId",
                    "COALESCE(con.nombre, '') as ConceptoNombre",
                    "con.id_categoria as CategoriaId",
                    "COALESCE(cat.nombre, '') as CategoriaNombre",
                    "ip.id_cliente as ClienteId",
                    "COALESCE(cli.nombre, '') as ClienteNombre",
                    "ip.id_persona as PersonaId",
                    "COALESCE(per.nombre, '') as PersonaNombre",
                    "ip.id_cuenta as CuentaId",
                    "COALESCE(cta.nombre, '') as CuentaNombre",
                    "ip.id_forma_pago as FormaPagoId",
                    "COALESCE(fp.nombre, '') as FormaPagoNombre",
                    "ip.id_usuario as UsuarioId"
                },
                joinClause: @"LEFT JOIN conceptos con ON ip.id_concepto = con.id
LEFT JOIN categorias cat ON con.id_categoria = cat.id
LEFT JOIN clientes cli ON ip.id_cliente = cli.id
LEFT JOIN personas per ON ip.id_persona = per.id
LEFT JOIN cuentas cta ON ip.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON ip.id_forma_pago = fp.id",
                searchableColumns: new List<string>
                {
                    "ip.descripcion",
                    "con.nombre",
                    "cat.nombre",
                    "cli.nombre",
                    "per.nombre",
                    "cta.nombre",
                    "fp.nombre",
                    "ip.frecuencia"
                },
                numericSearchableColumns: new List<string>
                {
                    "ip.importe"
                },
                dateSearchableColumns: new List<string>
                {
                    "ip.fecha_ejecucion"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "FechaEjecucion", "ip.fecha_ejecucion" },
                    { "Importe", "ip.importe" },
                    { "ConceptoNombre", "con.nombre" },
                    { "CategoriaNombre", "cat.nombre" },
                    { "ClienteNombre", "cli.nombre" },
                    { "PersonaNombre", "per.nombre" },
                    { "CuentaNombre", "cta.nombre" },
                    { "FormaPagoNombre", "fp.nombre" },
                    { "Frecuencia", "ip.frecuencia" },
                    { "Activo", "ip.activo" }
                },
                defaultOrderBy: "ip.fecha_ejecucion DESC, ip.id DESC"
            );
        }
    }
}