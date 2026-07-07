using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Results;

namespace Kash.Shared.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz principal para repositorios de lectura optimizados con DTOs.
    /// Permite obtener DTOs directamente desde la base de datos sin mapeo intermedio.
    /// Esta es la ÚNICA interfaz de lectura que debe usarse en la aplicación.
    /// </summary>
    public interface IReadRepository<T, TDto, TId>
        where T : AbsEntity<TId>
        where TDto : class
        where TId : IGuidValueObject
    {
        /// <summary>
        /// OPTIMIZADO: Obtiene un DTO por ID directamente desde la base de datos.
        /// </summary>
        Task<TDto?> GetReadModelByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// OPTIMIZADO: Obtiene todos los DTOs directamente desde la base de datos.
        /// </summary>
        Task<IEnumerable<TDto>> GetAllReadModelsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// OPTIMIZADO: Obtiene una página de DTOs directamente desde la base de datos.
        /// Evita el mapeo de Value Objects y mejora el rendimiento.
        /// </summary>
        Task<PagedList<TDto>> GetPagedReadModelsAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// OPTIMIZADO: Obtiene una página de DTOs filtrada por usuario con búsqueda y ordenamiento.
        /// Usa índices en la base de datos para máximo rendimiento (~50ms vs 370ms).
        /// </summary>
        Task<PagedList<TDto>> GetPagedReadModelsByUserAsync(
            Guid usuarioId,
            int page,
            int pageSize,
            string? searchTerm = null,
            string? sortColumn = null,
            string? sortOrder = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// NUEVO: Búsqueda rápida para autocomplete (limitada a pocos resultados).
        /// Ideal para selectores asíncronos que necesitan respuestas ultra-rápidas (<10ms).
        /// </summary>
        /// <param name="usuarioId">ID del usuario propietario</param>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <param name="limit">Número máximo de resultados (por defecto 10)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista limitada de DTOs que coinciden con la búsqueda</returns>
        Task<IEnumerable<TDto>> SearchForAutocompleteAsync(
            Guid usuarioId,
            string searchTerm,
            int limit = 10,
            Dictionary<string, object>? extraFilters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// NUEVO: Obtiene los elementos más recientes de un usuario.
        /// Ideal para pre-cargar selectores con los elementos usados recientemente.
        /// Ordenado por fecha_creacion DESC.
        /// </summary>
        /// <param name="usuarioId">ID del usuario propietario</param>
        /// <param name="limit">Número máximo de resultados (por defecto 5)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de los elementos más recientes</returns>
        Task<IEnumerable<TDto>> GetRecentAsync(
            Guid usuarioId,
            int limit = 5,
            Dictionary<string, object>? extraFilters = null,
            CancellationToken cancellationToken = default);
    }
}
