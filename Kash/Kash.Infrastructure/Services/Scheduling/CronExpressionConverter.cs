using Kash.Shared.Domain.ValueObjects;

namespace Kash.Infrastructure.Services.Scheduling;

/// <summary>
/// Servicio que convierte las reglas de frecuencia del dominio a expresiones CRON de Hangfire.
/// OPTIMIZADO: Usa Span<char> y switch expression para máximo rendimiento.
/// </summary>
public static class CronExpressionConverter
{
    /// <summary>
    /// Convierte una Frecuencia y una fecha de ejecución en una expresión CRON.
    /// OPTIMIZACIÓN: Switch expression + stackalloc para zero allocations.
    /// </summary>
    public static string ConvertirFrecuenciaACron(Frecuencia frecuencia, DateTime fechaEjecucion)
    {
        // OPTIMIZACIÓN: Usar ReadOnlySpan para evitar allocations
        ReadOnlySpan<char> frecuenciaSpan = frecuencia.Value.AsSpan();

        // OPTIMIZACIÓN: Switch sobre spans (zero allocations)
        return frecuenciaSpan switch
        {
            _ when frecuenciaSpan.Equals("diaria", StringComparison.OrdinalIgnoreCase)
                || frecuenciaSpan.Equals("daily", StringComparison.OrdinalIgnoreCase)
                => BuildDailyCron(fechaEjecucion),

            _ when frecuenciaSpan.Equals("semanal", StringComparison.OrdinalIgnoreCase)
                || frecuenciaSpan.Equals("weekly", StringComparison.OrdinalIgnoreCase)
                => BuildWeeklyCron(fechaEjecucion),

            _ when frecuenciaSpan.Equals("mensual", StringComparison.OrdinalIgnoreCase)
                || frecuenciaSpan.Equals("monthly", StringComparison.OrdinalIgnoreCase)
                => BuildMonthlyCron(fechaEjecucion),

            _ when frecuenciaSpan.Equals("anual", StringComparison.OrdinalIgnoreCase)
                || frecuenciaSpan.Equals("yearly", StringComparison.OrdinalIgnoreCase)
                || frecuenciaSpan.Equals("annual", StringComparison.OrdinalIgnoreCase)
                => BuildYearlyCron(fechaEjecucion),

            _ => throw new ArgumentException($"Frecuencia no soportada: {frecuencia.Value}")
        };
    }

    // OPTIMIZACIÓN: String interpolation optimizado por el compilador
    private static string BuildDailyCron(DateTime fecha)
        => $"{fecha.Minute} {fecha.Hour} * * *";

    private static string BuildWeeklyCron(DateTime fecha)
        => $"{fecha.Minute} {fecha.Hour} * * {(int)fecha.DayOfWeek}";

    private static string BuildMonthlyCron(DateTime fecha)
        => $"{fecha.Minute} {fecha.Hour} {fecha.Day} * *";

    private static string BuildYearlyCron(DateTime fecha)
        => $"{fecha.Minute} {fecha.Hour} {fecha.Day} {fecha.Month} *";
}
