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

 /// <summary>
    /// 🔥 Alias de la tabla principal para usar en JOINs.
   /// </summary>
  protected override string GetTableAlias()
      {
    return "g";
  }

        /// <summary>
        /// 🔥 Query de conteo que usa el alias correcto y los JOINs necesarios para la búsqueda.
        /// </summary>
        protected override string BuildCountQuery()
   {
         // 🔥 IMPORTANTE: Incluir los JOINs necesarios para que la búsqueda funcione
     return @"SELECT COUNT(*) FROM gastos g
LEFT JOIN conceptos c ON g.id_concepto = c.id
LEFT JOIN categorias cat ON c.id_categoria = cat.id
LEFT JOIN proveedores prov ON g.id_proveedor = prov.id
LEFT JOIN personas p ON g.id_persona = p.id
LEFT JOIN cuentas cta ON g.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON g.id_forma_pago = fp.id";
  }

        /// <summary>
        /// 🔥 Query optimizado para obtener un gasto con todos sus datos relacionados.
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
    cat.id as CategoriaId,
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

        /// <summary>
        /// 🔥 Query optimizado para obtener todos los gastos con sus datos relacionados.
        /// </summary>
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
    cat.id as CategoriaId,
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

        /// <summary>
        /// 🔥 IMPORTANTE: Query para paginación (debe incluir los mismos JOINs que BuildGetAllQuery).
        /// </summary>
   protected override string BuildGetPagedQuery()
 {
          return BuildGetAllQuery(); // Reutilizar el query completo con JOINs
        }

        /// <summary>
  /// 🔥 ORDER BY por defecto: ordenar por fecha descendente (más recientes primero).
        /// </summary>
        protected override string GetDefaultOrderBy()
        {
 return "ORDER BY g.fecha DESC, g.id DESC";
        }

        /// <summary>
        /// 🔥 NUEVO: Define las columnas por las que se puede ordenar.
        /// </summary>
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

        /// <summary>
        /// 🔥 NUEVO: Define las columnas de TEXTO en las que se puede buscar con LIKE.
  /// </summary>
   protected override List<string> GetSearchableColumns()
      {
         return new List<string>
        {
         "g.descripcion",      // Descripción del gasto
"c.nombre",           // Nombre del concepto
 "cat.nombre",       // Nombre de la categoría
     "prov.nombre",      // Nombre del proveedor
            "p.nombre",  // Nombre de la persona
  "cta.nombre"          // Nombre de la cuenta
   };
        }

        /// <summary>
        /// 🔥 NUEVO: Define las columnas NUMÉRICAS en las que se puede buscar.
        /// Permite buscar gastos por importe exacto (ej: buscar "50" encuentra gastos de 50.00).
        /// </summary>
        protected override List<string> GetNumericSearchableColumns()
        {
   return new List<string>
            {
       "g.importe"  // Buscar por importe exacto
            };
        }

        /// <summary>
        /// 🔥 NUEVO: Define las columnas de FECHA en las que se puede buscar.
/// Permite buscar por fecha completa o parcial (ej: "2024", "2024-01", "2024-01-15").
        /// </summary>
        protected override List<string> GetDateSearchableColumns()
        {
    return new List<string>
            {
         "g.fecha"  // Buscar por fecha
     };
        }
    }
}