using Kash.Domain;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Ingresos
{
    public class IngresoReadRepository : AbsReadRepository<Ingreso, IngresoDto, IngresoId>, IIngresoReadRepository
    {
        public IngresoReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.WithJoins(
                tableName: "ingresos",
                tableAlias: "i",
                selectColumns: new List<string>
                {
                    "i.id as Id",
                    "i.importe as Importe",
                    "i.fecha as Fecha",
                    "i.descripcion as Descripcion",
                    "i.id_concepto as ConceptoId",
                    "COALESCE(c.nombre, '') as ConceptoNombre",
                    "cat.id as CategoriaId",
                    "cat.nombre as CategoriaNombre",
                    "i.id_cliente as ClienteId",
                    "COALESCE(cli.nombre, '') as ClienteNombre",
                    "i.id_persona as PersonaId",
                    "COALESCE(p.nombre, '') as PersonaNombre",
                    "i.id_cuenta as CuentaId",
                    "COALESCE(cta.nombre, '') as CuentaNombre",
                    "i.id_forma_pago as FormaPagoId",
                    "COALESCE(fp.nombre, '') as FormaPagoNombre",
                    "i.id_usuario as UsuarioId"
                },
                joinClause: @"LEFT JOIN conceptos c ON i.id_concepto = c.id
LEFT JOIN categorias cat ON c.id_categoria = cat.id
LEFT JOIN clientes cli ON i.id_cliente = cli.id
LEFT JOIN personas p ON i.id_persona = p.id
LEFT JOIN cuentas cta ON i.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON i.id_forma_pago = fp.id",
                searchableColumns: new List<string>
                {
                    "i.descripcion",
                    "c.nombre",
                    "cat.nombre",
                    "cli.nombre",
                    "p.nombre",
                    "cta.nombre"
                },
                numericSearchableColumns: new List<string>
                {
                    "i.importe"
                },
                dateSearchableColumns: new List<string>
                {
                    "i.fecha"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Fecha", "i.fecha" },
                    { "Importe", "i.importe" },
                    { "ConceptoNombre", "c.nombre" },
                    { "CategoriaNombre", "cat.nombre" },
                    { "ClienteNombre", "cli.nombre" },
                    { "PersonaNombre", "p.nombre" },
                    { "CuentaNombre", "cta.nombre" },
                    { "FormaPagoNombre", "fp.nombre" }
                },
                defaultOrderBy: "i.fecha DESC, i.id DESC"
            );
        }
    }
}
