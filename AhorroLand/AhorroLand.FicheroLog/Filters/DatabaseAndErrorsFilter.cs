using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace AhorroLand.FicheroLog.Filters;

/// <summary>
/// Filtro que solo permite logs importantes:
/// - Operaciones de escritura/lectura en base de datos
/// - Warnings del sistema
/// - Errores y excepciones
/// </summary>
public sealed class DatabaseAndErrorsFilter
{
    private static readonly HashSet<string> DatabaseKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "INSERT", "UPDATE", "DELETE", "SELECT",
        "Query", "Command", "ExecuteNonQuery", "ExecuteReader", "ExecuteScalar",
        "SaveChanges", "Database", "SQL", "MySQL", "Transaction",
        "Repository", "DbContext", "Entity"
    };

    public static bool ShouldInclude(LogEvent logEvent, bool includeDatabaseOps, bool includeWarnings, bool includeErrors)
    {
        // Siempre incluir errores y excepciones si está configurado
        if (includeErrors && logEvent.Level >= LogEventLevel.Error)
        {
            return true;
        }

        // Incluir warnings si está configurado
        if (includeWarnings && logEvent.Level == LogEventLevel.Warning)
        {
            return true;
        }

        // Solo procesar Information si se requieren operaciones de BD
        if (!includeDatabaseOps || logEvent.Level != LogEventLevel.Information)
        {
            return false;
        }

        // Verificar si el mensaje contiene palabras clave de base de datos
        var messageTemplate = logEvent.MessageTemplate.Text;
        var renderedMessage = logEvent.RenderMessage();

        if (ContainsDatabaseKeyword(messageTemplate) || ContainsDatabaseKeyword(renderedMessage))
        {
            return true;
        }

        // Verificar propiedades del log
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            var sourceContextValue = sourceContext.ToString().Trim('"');
            if (ContainsDatabaseKeyword(sourceContextValue))
            {
                return true;
            }
        }

        // Verificar si hay propiedades relacionadas con operaciones de BD
        foreach (var property in logEvent.Properties)
        {
            if (ContainsDatabaseKeyword(property.Key) || 
                ContainsDatabaseKeyword(property.Value.ToString()))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsDatabaseKeyword(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return DatabaseKeywords.Any(keyword => 
            text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
