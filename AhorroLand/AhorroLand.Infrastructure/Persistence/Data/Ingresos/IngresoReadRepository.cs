using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;

namespace AhorroLand.Infrastructure.Persistence.Data.Ingresos
{
    public class IngresoReadRepository : AbsReadRepository<Ingreso, IngresoDto>, IIngresoReadRepository
    {
        public IngresoReadRepository(IDbConnectionFactory dbConnectionFactory)
            : base(dbConnectionFactory, "ingresos")
        {
        }

        /// <summary>
        /// 🔥 Alias de la tabla principal para usar en JOINs.
        /// </summary>
        protected override string GetTableAlias()
        {
            return "i";
        }

        /// <summary>
        /// 🔥 Query de conteo que usa el alias correcto y los JOINs necesarios para la búsqueda.
        /// </summary>
        protected override string BuildCountQuery()
        {
            // 🔥 IMPORTANTE: Incluir los JOINs necesarios para que la búsqueda funcione
            return @"SELECT COUNT(*) FROM ingresos i
LEFT JOIN conceptos c ON i.id_concepto = c.id
LEFT JOIN categorias cat ON c.id_categoria = cat.id
LEFT JOIN clientes cli ON i.id_cliente = cli.id
LEFT JOIN personas p ON i.id_persona = p.id
LEFT JOIN cuentas cta ON i.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON i.id_forma_pago = fp.id";
        }

        /// <summary>
        /// 🔥 Query optimizado para obtener un ingreso con todos sus datos relacionados.
        /// </summary>
        protected override string BuildGetByIdQuery()
        {
            return @"
SELECT 
  i.id as Id,
    i.importe as Importe,
    i.fecha as Fecha,
    i.descripcion as Descripcion,
    i.id_concepto as ConceptoId,
    COALESCE(c.nombre, '') as ConceptoNombre,
cat.id as CategoriaId,
    cat.nombre as CategoriaNombre,
    i.id_cliente as ClienteId,
    COALESCE(cli.nombre, '') as ClienteNombre,
    i.id_persona as PersonaId,
    COALESCE(p.nombre, '') as PersonaNombre,
    i.id_cuenta as CuentaId,
    COALESCE(cta.nombre, '') as CuentaNombre,
    i.id_forma_pago as FormaPagoId,
    COALESCE(fp.nombre, '') as FormaPagoNombre,
    i.id_usuario as UsuarioId
FROM ingresos i
LEFT JOIN conceptos c ON i.id_concepto = c.id
LEFT JOIN categorias cat ON c.id_categoria = cat.id
LEFT JOIN clientes cli ON i.id_cliente = cli.id
LEFT JOIN personas p ON i.id_persona = p.id
LEFT JOIN cuentas cta ON i.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON i.id_forma_pago = fp.id
WHERE i.id = @id";
        }

        /// <summary>
        /// 🔥 Query optimizado para obtener todos los ingresos con sus datos relacionados.
        /// </summary>
        protected override string BuildGetAllQuery()
        {
            return @"
SELECT 
    i.id as Id,
  i.importe as Importe,
    i.fecha as Fecha,
    i.descripcion as Descripcion,
    i.id_concepto as ConceptoId,
    COALESCE(c.nombre, '') as ConceptoNombre,
    cat.id as CategoriaId,
    cat.nombre as CategoriaNombre,
    i.id_cliente as ClienteId,
 COALESCE(cli.nombre, '') as ClienteNombre,
    i.id_persona as PersonaId,
    COALESCE(p.nombre, '') as PersonaNombre,
 i.id_cuenta as CuentaId,
    COALESCE(cta.nombre, '') as CuentaNombre,
    i.id_forma_pago as FormaPagoId,
    COALESCE(fp.nombre, '') as FormaPagoNombre,
    i.id_usuario as UsuarioId
FROM ingresos i
LEFT JOIN conceptos c ON i.id_concepto = c.id
LEFT JOIN categorias cat ON c.id_categoria = cat.id
LEFT JOIN clientes cli ON i.id_cliente = cli.id
LEFT JOIN personas p ON i.id_persona = p.id
LEFT JOIN cuentas cta ON i.id_cuenta = cta.id
LEFT JOIN formas_pago fp ON i.id_forma_pago = fp.id";
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
            return "ORDER BY i.fecha DESC, i.id DESC";
   }

    /// <summary>
        /// 🔥 NUEVO: Define las columnas por las que se puede ordenar.
        /// </summary>
        protected override Dictionary<string, string> GetSortableColumns()
        {
      return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
     {
       { "Fecha", "i.fecha" },
        { "Importe", "i.importe" },
   { "ConceptoNombre", "c.nombre" },
      { "CategoriaNombre", "cat.nombre" },
        { "ClienteNombre", "cli.nombre" },
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
    "i.descripcion",      // Descripción del ingreso
       "c.nombre", // Nombre del concepto
   "cat.nombre",    // Nombre de la categoría
  "cli.nombre",         // Nombre del cliente
    "p.nombre",           // Nombre de la persona
"cta.nombre"          // Nombre de la cuenta
         };
        }

        /// <summary>
        /// 🔥 NUEVO: Define las columnas NUMÉRICAS en las que se puede buscar.
        /// </summary>
        protected override List<string> GetNumericSearchableColumns()
        {
    return new List<string>
   {
        "i.importe"  // Buscar por importe exacto
};
        }

        /// <summary>
        /// 🔥 NUEVO: Define las columnas de FECHA en las que se puede buscar.
     /// </summary>
        protected override List<string> GetDateSearchableColumns()
        {
   return new List<string>
      {
  "i.fecha"  // Buscar por fecha
     };
        }
 }
}