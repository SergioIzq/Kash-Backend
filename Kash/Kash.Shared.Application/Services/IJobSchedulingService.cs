using System.Linq.Expressions;

namespace Kash.Shared.Application.Abstractions.Servicies;

/// <summary>
/// Servicio de aplicación para la gestión de trabajos programados (Hangfire, Quartz, etc.).
/// Abstrae la lógica de programación, actualización y eliminación de tareas en segundo plano.
/// </summary>
public interface IJobSchedulingService
{
    /// <summary>
    /// Genera un identificador único para un trabajo programado.
    /// </summary>
    /// <returns>Un string único que representa el Job ID.</returns>
    string GenerateJobId();

    /// <summary>
    /// Programa un trabajo recurrente con una expresión cron.
    /// </summary>
    /// <param name="jobId">Identificador único del trabajo</param>
    /// <param name="fechaInicio">Fecha de inicio del trabajo</param>
    /// <param name="frecuencia">Frecuencia en formato cron (ej: "0 0 * * *" para diario)</param>
    /// <param name="methodCall">Expresión del método a ejecutar</param>
    /// <returns>Task completado cuando el job se haya programado</returns>
    Task ScheduleRecurringJobAsync(
        string jobId,
        DateTime fechaInicio,
        string frecuencia,
        Expression<Func<Task>> methodCall);

    /// <summary>
    /// Elimina un trabajo recurrente programado.
    /// </summary>
    /// <param name="jobId">Identificador del trabajo a eliminar</param>
    /// <returns>Task completado cuando el job se haya eliminado</returns>
    Task RemoveRecurringJobAsync(string jobId);

    /// <summary>
    /// Actualiza un trabajo recurrente existente (elimina el anterior y crea uno nuevo).
    /// </summary>
    /// <param name="jobId">Identificador del trabajo</param>
    /// <param name="fechaInicio">Nueva fecha de inicio</param>
    /// <param name="frecuencia">Nueva frecuencia en formato cron</param>
    /// <param name="methodCall">Nueva expresión del método a ejecutar</param>
    /// <returns>Task completado cuando el job se haya actualizado</returns>
    Task UpdateRecurringJobAsync(
        string jobId,
        DateTime fechaInicio,
        string frecuencia,
        Expression<Func<Task>> methodCall);

    /// <summary>
    /// Verifica si existe un trabajo recurrente con el ID especificado.
    /// </summary>
    /// <param name="jobId">Identificador del trabajo</param>
    /// <returns>True si el job existe, false en caso contrario</returns>
    bool JobExists(string jobId);
}

