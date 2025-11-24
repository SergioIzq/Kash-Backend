using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;

namespace AhorroLand.Infrastructure.Persistence.Data.Gastos
{
    public class GastoReadRepository : AbsReadRepository<Gasto, GastoDto>, IGastoReadRepository
    {
        public GastoReadRepository(IDbConnectionFactory dbConnectionFactory)
          : base(dbConnectionFactory, "gastos")
        {
        }

     protected override string GetTableAlias()
        {
   return "g";
        }

 protected override string BuildCountQuery()
    {
            return @"SELECT COUNT(*) FROM gastos g
LEFT JOIN conceptos c ON g.id_concepto = c.id
LEFT JOIN proveedores prov ON g.id_proveedor = prov.id
LEFT JOIN personas p ON g.id_persona = p.id
LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON g.id_forma_pago = fp.id";
  }

        /// <summary>
        /// ✅ CORRECTO: CategoriaId se obtiene del CONCEPTO (c.categoria_id), no del gasto
        /// </summary>
    protected override string BuildGetByIdQuery()
     {
            return @"
SELECT 
    g.id as Id,
    g.importe as Importe,
    g.fecha as Fecha,
    g.descripcion as Descripcion,
    g.id_concepto as ConceptoId,
    COALESCE(c.nombre, '') as ConceptoNombre,
    c.id_categoria as CategoriaId,
    cat.nombre as CategoriaNombre,
    g.id_proveedor as ProveedorId,
    prov.nombre as ProveedorNombre,
    g.id_persona as PersonaId,
    p.nombre as PersonaNombre,
    g.id_cuenta as CuentaId,
    COALESCE(cta.nombre, '') as CuentaNombre,
    g.id_forma_pago as FormaPagoId,
    COALESCE(fp.nombre, '') as FormaPagoNombre,
    g.id_usuario as UsuarioId
FROM gastos g
LEFT JOIN conceptos c ON g.id_concepto = c.id
LEFT JOIN categorias cat ON c.id_categoria = cat.id
LEFT JOIN proveedores prov ON g.id_proveedor = prov.id
LEFT JOIN personas p ON g.id_persona = p.id
LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON g.id_forma_pago = fp.id
WHERE g.id = @id";
        }

        protected override string BuildGetAllQuery()
        {
            return @"
SELECT 
    g.id as Id,
    g.importe as Importe,
    g.fecha as Fecha,
    g.descripcion as Descripcion,
    g.id_concepto as ConceptoId,
    COALESCE(c.nombre, '') as ConceptoNombre,
    c.id_categoria as CategoriaId,
    cat.nombre as CategoriaNombre,
    g.id_proveedor as ProveedorId,
    prov.nombre as ProveedorNombre,
    g.id_persona as PersonaId,
    p.nombre as PersonaNombre,
    g.id_cuenta as CuentaId,
    COALESCE(cta.nombre, '') as CuentaNombre,
    g.id_forma_pago as FormaPagoId,
    COALESCE(fp.nombre, '') as FormaPagoNombre,
    g.id_usuario as UsuarioId
FROM gastos g
LEFT JOIN conceptos c ON g.id_concepto = c.id
LEFT JOIN categorias cat ON c.id_categoria = cat.id
LEFT JOIN proveedores prov ON g.id_proveedor = prov.id
LEFT JOIN personas p ON g.id_persona = p.id
LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON g.id_forma_pago = fp.id";
        }

        protected override string BuildGetPagedQuery()
        {
     return BuildGetAllQuery();
        }

  protected override string GetDefaultOrderBy()
        {
  return "ORDER BY g.fecha DESC, g.id DESC";
      }

        protected override Dictionary<string, string> GetSortableColumns()
     {
   return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
     {
         { "Fecha", "g.fecha" },
        { "Importe", "g.importe" },
           { "ConceptoNombre", "c.nombre" },
              { "CategoriaNombre", "cat.nombre" },
  { "ProveedorNombre", "prov.nombre" },
         { "PersonaNombre", "p.nombre" },
       { "CuentaNombre", "cta.nombre" },
 { "FormaPagoNombre", "fp.nombre" }
            };
     }

        protected override List<string> GetSearchableColumns()
        {
            return new List<string>
    {
    "g.descripcion",
        "c.nombre",
                "cat.nombre",
       "prov.nombre",
      "p.nombre",
              "cta.nombre"
     };
      }

        protected override List<string> GetNumericSearchableColumns()
        {
            return new List<string>
            {
     "g.importe"
};
        }

        protected override List<string> GetDateSearchableColumns()
     {
       return new List<string>
      {
                "g.fecha"
   };
      }
    }
}