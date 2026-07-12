using Kash.Domain;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Gastos
{
    public class GastoReadRepository : AbsReadRepository<Gasto, GastoDto, GastoId>, IGastoReadRepository
    {
        public GastoReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory)
        {
        }

        /// <summary>
        /// ÚNICA CONFIGURACIÓN REQUERIDA: Define todas las características del repositorio.
        /// </summary>
        protected override ReadRepositoryConfiguration ConfigureRepository()
        {
            return ReadRepositoryConfiguration.WithJoins(
                tableName: "gastos",
                tableAlias: "g",
                selectColumns: new List<string>
                {
                    "g.id as Id",
                    "g.importe as Importe",
                    "g.fecha as Fecha",
                    "g.descripcion as Descripcion",
                    "g.id_concepto as ConceptoId",
                    "COALESCE(c.nombre, '') as ConceptoNombre",
                    "c.id_categoria as CategoriaId",
                    "cat.nombre as CategoriaNombre",
                    "g.id_proveedor as ProveedorId",
                    "prov.nombre as ProveedorNombre",
                    "g.id_persona as PersonaId",
                    "p.nombre as PersonaNombre",
                    "g.id_cuenta as CuentaId",
                    "COALESCE(cta.nombre, '') as CuentaNombre",
                    "g.id_forma_pago as FormaPagoId",
                    "COALESCE(fp.nombre, '') as FormaPagoNombre",
                    "g.id_usuario as UsuarioId"
                },
                joinClause: @"LEFT JOIN conceptos c ON g.id_concepto = c.id
LEFT JOIN categorias cat ON c.id_categoria = cat.id
LEFT JOIN proveedores prov ON g.id_proveedor = prov.id
LEFT JOIN personas p ON g.id_persona = p.id
LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON g.id_forma_pago = fp.id",
                searchableColumns: new List<string>
                {
                    "g.descripcion",
                    "c.nombre",
                    "cat.nombre",
                    "prov.nombre",
                    "p.nombre",
                    "cta.nombre"
                },
                numericSearchableColumns: new List<string>
                {
                    "g.importe"
                },
                dateSearchableColumns: new List<string>
                {
                    "g.fecha"
                },
                sortableColumns: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Fecha", "g.fecha" },
                    { "Importe", "g.importe" },
                    { "ConceptoNombre", "c.nombre" },
                    { "CategoriaNombre", "cat.nombre" },
                    { "ProveedorNombre", "prov.nombre" },
                    { "PersonaNombre", "p.nombre" },
                    { "CuentaNombre", "cta.nombre" },
                    { "FormaPagoNombre", "fp.nombre" }
                },
                defaultOrderBy: "g.fecha DESC, g.id DESC"
            );
        }
    }
}
