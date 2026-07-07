using Kash.Domain;
using Kash.Infrastructure.Persistence.Query;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.GastosProgramados
{
    public class GastoProgramadoReadRepository : AbsReadRepository<GastoProgramado, GastoProgramadoDto, GastoProgramadoId>, IGastoProgramadoReadRepository
    {
        public GastoProgramadoReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.WithJoins(
                tableName: "gastos_programados",
                tableAlias: "gp",
                selectColumns: new List<string>
                {
                    "gp.id as Id",
                    "gp.importe as Importe",
                    "gp.fecha_ejecucion as FechaEjecucion",
                    "gp.descripcion as Descripcion",
                    "gp.frecuencia as Frecuencia",
                    "gp.activo as Activo",
                    "gp.hangfire_job_id as HangfireJobId",
                    "gp.id_concepto as ConceptoId",
                    "COALESCE(con.nombre, '') as ConceptoNombre",
                    "con.id_categoria as CategoriaId",
                    "COALESCE(cat.nombre, '') as CategoriaNombre",
                    "gp.id_proveedor as ProveedorId",
                    "COALESCE(prov.nombre, '') as ProveedorNombre",
                    "gp.id_persona as PersonaId",
                    "COALESCE(per.nombre, '') as PersonaNombre",
                    "gp.id_cuenta as CuentaId",
                    "COALESCE(cta.nombre, '') as CuentaNombre",
                    "gp.id_forma_pago as FormaPagoId",
                    "COALESCE(fp.nombre, '') as FormaPagoNombre",
                    "gp.id_usuario as UsuarioId"
                },
                joinClause: @"LEFT JOIN conceptos con ON gp.id_concepto = con.id
LEFT JOIN categorias cat ON con.id_categoria = cat.id
LEFT JOIN proveedores prov ON gp.id_proveedor = prov.id
LEFT JOIN personas per ON gp.id_persona = per.id
LEFT JOIN cuentas cta ON gp.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON gp.id_forma_pago = fp.id",
                searchableColumns: new List<string>
                {
                    "gp.descripcion",
                    "con.nombre",
                    "cat.nombre",
                    "prov.nombre",
                    "per.nombre",
                    "cta.nombre",
                    "fp.nombre",
                    "gp.frecuencia"
                },
                numericSearchableColumns: new List<string>
                {
                    "gp.importe"
                },
                dateSearchableColumns: new List<string>
                {
                    "gp.fecha_ejecucion"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "FechaEjecucion", "gp.fecha_ejecucion" },
                    { "Importe", "gp.importe" },
                    { "ConceptoNombre", "con.nombre" },
                    { "CategoriaNombre", "cat.nombre" },
                    { "ProveedorNombre", "prov.nombre" },
                    { "PersonaNombre", "per.nombre" },
                    { "CuentaNombre", "cta.nombre" },
                    { "FormaPagoNombre", "fp.nombre" },
                    { "Frecuencia", "gp.frecuencia" },
                    { "Activo", "gp.activo" }
                },
                defaultOrderBy: "gp.fecha_ejecucion DESC, gp.id DESC"
            );
        }
    }
}
