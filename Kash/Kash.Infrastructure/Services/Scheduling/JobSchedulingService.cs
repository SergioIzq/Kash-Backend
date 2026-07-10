using Hangfire;
using SergioIzq.Application.Kernel.Services;
using System.Linq.Expressions;

namespace Kash.Infrastructure.Services.Scheduling;

/// <summary>
/// Implementaci�n del servicio de programaci�n de trabajos usando Hangfire.
/// Gestiona la creaci�n, actualizaci�n y eliminaci�n de jobs recurrentes.
/// </summary>
public sealed class JobSchedulingService : IJobSchedulingService
{
    /// <summary>
    /// Genera un identificador �nico para un trabajo programado.
    /// </summary>
    /// <returns>Un string �nico basado en GUID.</returns>
    public string GenerateJobId()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Programa un trabajo recurrente en Hangfire.
    /// </summary>
    /// <param name="jobId">Identificador �nico del trabajo</param>
    /// <param name="fechaInicio">Fecha de inicio (no usada en Hangfire recurrente, pero �til para logs)</param>
    /// <param name="frecuencia">Frecuencia en formato cron (ej: "0 0 * * *" para diario a medianoche)</param>
    /// <param name="methodCall">Expresi�n del m�todo a ejecutar</param>
    public Task ScheduleRecurringJobAsync(
        string jobId,
        DateTime fechaInicio,
        string frecuencia,
        Expression<Func<Task>> methodCall)
    {
        // RecurringJob.AddOrUpdate crea o actualiza el job
        RecurringJob.AddOrUpdate(
            recurringJobId: jobId,
            methodCall: methodCall,
            cronExpression: frecuencia,
            options: new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local // Usar zona horaria local del servidor
            });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Elimina un trabajo recurrente de Hangfire.
    /// </summary>
    /// <param name="jobId">Identificador del trabajo a eliminar</param>
    public Task RemoveRecurringJobAsync(string jobId)
    {
        RecurringJob.RemoveIfExists(jobId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Actualiza un trabajo recurrente (elimina y recrea).
    /// En Hangfire, AddOrUpdate ya actualiza, pero este m�todo es expl�cito.
    /// </summary>
    public async Task UpdateRecurringJobAsync(
        string jobId,
        DateTime fechaInicio,
        string frecuencia,
        Expression<Func<Task>> methodCall)
    {
        // Eliminar el job anterior
        await RemoveRecurringJobAsync(jobId);

        // Crear el nuevo job con los datos actualizados
        await ScheduleRecurringJobAsync(jobId, fechaInicio, frecuencia, methodCall);
    }

    /// <summary>
    /// Verifica si existe un trabajo recurrente en Hangfire.
    /// Nota: Hangfire no proporciona una API directa para verificar existencia.
    /// Este m�todo intenta triggear el job para verificar si existe.
    /// </summary>
    /// <param name="jobId">Identificador del trabajo</param>
    /// <returns>True si el job existe, false en caso contrario</returns>
    public bool JobExists(string jobId)
    {
        try
        {
            // Intenta triggear el job de forma inmediata para verificar que existe
            // Si no existe, lanzaruna excepcin
            RecurringJob.TriggerJob(jobId);
            return true;
        }
        catch
        {
            // Si falla, el job no existe
            return false;
        }
    }
}

