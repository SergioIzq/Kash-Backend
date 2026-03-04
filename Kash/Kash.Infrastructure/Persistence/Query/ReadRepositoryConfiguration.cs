namespace Kash.Infrastructure.Persistence.Query
{
    /// <summary>
    /// ?? Configuración declarativa para AbsReadRepository.
    /// Elimina la necesidad de sobrescribir múltiples métodos virtuales.
    /// </summary>
    public class ReadRepositoryConfiguration
    {
        /// <summary>
        /// Nombre de la tabla principal (requerido)
        /// </summary>
        public required string TableName { get; init; }

        /// <summary>
        /// Alias de la tabla principal (opcional, solo para JOINs)
        /// Ejemplo: "g" para gastos, "i" para ingresos
        /// </summary>
        public string? TableAlias { get; init; }

        /// <summary>
        /// Columnas a seleccionar en las queries.
        /// Si no se especifica, usa las columnas básicas: id, id_usuario, fecha_creacion
        /// </summary>
        public List<string>? SelectColumns { get; init; }

        /// <summary>
        /// Cláusula JOIN completa para queries con relaciones.
        /// Ejemplo: "LEFT JOIN conceptos c ON g.id_concepto = c.id"
        /// </summary>
        public string? JoinClause { get; init; }

        /// <summary>
        /// Columnas de texto sobre las que se puede buscar con LIKE
        /// Ejemplo: ["nombre", "descripcion"]
        /// </summary>
        public List<string>? SearchableColumns { get; init; }

        /// <summary>
        /// Columnas numéricas sobre las que se puede buscar con comparación exacta
        /// Ejemplo: ["importe", "cantidad"]
        /// </summary>
        public List<string>? NumericSearchableColumns { get; init; }

        /// <summary>
        /// Columnas de fecha sobre las que se puede buscar
        /// Ejemplo: ["fecha", "fecha_registro"]
        /// </summary>
        public List<string>? DateSearchableColumns { get; init; }

        /// <summary>
        /// Mapeo de nombres de columnas para ordenamiento.
        /// Key: Nombre en DTO (ej: "ConceptoNombre")
        /// Value: Nombre en BD (ej: "c.nombre")
        /// </summary>
        public Dictionary<string, string>? SortableColumns { get; init; }

        /// <summary>
        /// Orden por defecto para paginación.
        /// Ejemplo: "fecha DESC, id DESC"
        /// </summary>
        public string? DefaultOrderBy { get; init; }

        /// <summary>
        /// Nombre de la columna de usuario para filtros.
        /// Por defecto: "id_usuario"
        /// Si hay alias, se usa automáticamente: "{alias}.id_usuario"
        /// </summary>
        public string? UserIdColumn { get; init; }

        /// <summary>
        /// Factory method para entidades simples sin JOINs
        /// </summary>
        public static ReadRepositoryConfiguration Simple(
            string tableName,
            List<string> selectColumns,
            List<string>? searchableColumns = null,
            Dictionary<string, string>? sortableColumns = null,
            string? defaultOrderBy = null)
        {
            return new ReadRepositoryConfiguration
            {
                TableName = tableName,
                SelectColumns = selectColumns,
                SearchableColumns = searchableColumns,
                SortableColumns = sortableColumns,
                DefaultOrderBy = defaultOrderBy
            };
        }

        /// <summary>
        /// Factory method para entidades con JOINs
        /// </summary>
        public static ReadRepositoryConfiguration WithJoins(
            string tableName,
            string tableAlias,
            List<string> selectColumns,
            string joinClause,
            List<string>? searchableColumns = null,
            List<string>? numericSearchableColumns = null,
            List<string>? dateSearchableColumns = null,
            Dictionary<string, string>? sortableColumns = null,
            string? defaultOrderBy = null)
        {
            return new ReadRepositoryConfiguration
            {
                TableName = tableName,
                TableAlias = tableAlias,
                SelectColumns = selectColumns,
                JoinClause = joinClause,
                SearchableColumns = searchableColumns,
                NumericSearchableColumns = numericSearchableColumns,
                DateSearchableColumns = dateSearchableColumns,
                SortableColumns = sortableColumns,
                DefaultOrderBy = defaultOrderBy
            };
        }
    }
}
