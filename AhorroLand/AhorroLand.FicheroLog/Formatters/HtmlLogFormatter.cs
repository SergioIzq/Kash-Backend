using Serilog.Events;
using Serilog.Formatting;
using System.Text;

namespace AhorroLand.FicheroLog.Formatters;

/// <summary>
/// Formateador que genera logs en formato HTML con diseño visual mejorado
/// </summary>
public sealed class HtmlLogFormatter : ITextFormatter
{
    private readonly string _pageTitle;
    private static bool _headerWritten;
    private static readonly object _lock = new();

    public HtmlLogFormatter(string pageTitle = "Logs del Sistema")
    {
        _pageTitle = pageTitle;
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        lock (_lock)
        {
            // Escribir header HTML solo una vez al inicio del archivo
            if (!_headerWritten)
            {
                WriteHtmlHeader(output);
                _headerWritten = true;
            }
        }

        WriteLogEntry(logEvent, output);
    }

    private void WriteHtmlHeader(TextWriter output)
    {
        var html = $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{_pageTitle}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            min-height: 100vh;
        }}

        .container {{
            max-width: 1400px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            overflow: hidden;
        }}

        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}

        .header h1 {{
            font-size: 2.5em;
            margin-bottom: 10px;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
        }}

        .header .subtitle {{
            opacity: 0.9;
            font-size: 1.1em;
        }}

        .controls {{
            padding: 20px 30px;
            background: #f8f9fa;
            border-bottom: 2px solid #e9ecef;
            display: flex;
            gap: 15px;
            flex-wrap: wrap;
            align-items: center;
        }}

        .controls input {{
            padding: 10px 15px;
            border: 2px solid #dee2e6;
            border-radius: 6px;
            font-size: 14px;
            flex: 1;
            min-width: 200px;
        }}

        .controls select, .controls button {{
            padding: 10px 20px;
            border: 2px solid #667eea;
            border-radius: 6px;
            font-size: 14px;
            cursor: pointer;
            background: white;
            color: #667eea;
            font-weight: 600;
            transition: all 0.3s;
        }}

        .controls button:hover {{
            background: #667eea;
            color: white;
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(102, 126, 234, 0.3);
        }}

        .logs {{
            padding: 30px;
        }}

        .log-entry {{
            margin-bottom: 20px;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            transition: all 0.3s;
            animation: slideIn 0.3s ease-out;
        }}

        @keyframes slideIn {{
            from {{
                opacity: 0;
                transform: translateY(-10px);
            }}
            to {{
                opacity: 1;
                transform: translateY(0);
            }}
        }}

        .log-entry:hover {{
            transform: translateY(-4px);
            box-shadow: 0 8px 12px rgba(0,0,0,0.15);
        }}

        .log-header {{
            padding: 15px 20px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-weight: 600;
            color: white;
        }}

        .log-header .level {{
            padding: 5px 15px;
            border-radius: 20px;
            font-size: 0.85em;
            text-transform: uppercase;
            letter-spacing: 1px;
            background: rgba(255,255,255,0.2);
        }}

        .log-body {{
            padding: 20px;
            background: white;
        }}

        .log-message {{
            font-size: 1.1em;
            line-height: 1.6;
            color: #2c3e50;
            margin-bottom: 15px;
            font-weight: 500;
        }}

        .log-properties {{
            background: #f8f9fa;
            padding: 15px;
            border-radius: 6px;
            border-left: 4px solid #667eea;
        }}

        .property {{
            margin: 8px 0;
            padding: 8px;
            background: white;
            border-radius: 4px;
            font-family: 'Courier New', monospace;
            font-size: 0.9em;
        }}

        .property-name {{
            font-weight: 700;
            color: #667eea;
        }}

        .property-value {{
            color: #495057;
            margin-left: 10px;
        }}

        .exception {{
            background: #fff5f5;
            border: 2px solid #fc8181;
            border-radius: 6px;
            padding: 15px;
            margin-top: 15px;
            font-family: 'Courier New', monospace;
            font-size: 0.9em;
            color: #c53030;
            white-space: pre-wrap;
            word-wrap: break-word;
        }}

        .log-entry.error .log-header {{
            background: linear-gradient(135deg, #fc8181 0%, #e53e3e 100%);
        }}

        .log-entry.warning .log-header {{
            background: linear-gradient(135deg, #f6ad55 0%, #ed8936 100%);
        }}

        .log-entry.info .log-header {{
            background: linear-gradient(135deg, #4299e1 0%, #3182ce 100%);
        }}

        .log-entry.debug .log-header {{
            background: linear-gradient(135deg, #68d391 0%, #38b2ac 100%);
        }}

        .stats {{
            padding: 20px 30px;
            background: #f8f9fa;
            border-top: 2px solid #e9ecef;
            display: flex;
            justify-content: space-around;
            flex-wrap: wrap;
            gap: 20px;
        }}

        .stat-item {{
            text-align: center;
        }}

        .stat-value {{
            font-size: 2em;
            font-weight: bold;
            color: #667eea;
        }}

        .stat-label {{
            color: #6c757d;
            margin-top: 5px;
        }}

        .hidden {{
            display: none;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? {_pageTitle}</h1>
            <div class='subtitle'>Sistema de Monitoreo de Logs</div>
        </div>
        
        <div class='controls'>
            <input type='text' id='searchBox' placeholder='?? Buscar en los logs...' onkeyup='filterLogs()'>
            <select id='levelFilter' onchange='filterLogs()'>
                <option value='all'>Todos los niveles</option>
                <option value='error'>? Solo Errores</option>
                <option value='warning'>?? Solo Warnings</option>
                <option value='info'>?? Solo Info</option>
                <option value='debug'>?? Solo Debug</option>
            </select>
            <button onclick='clearFilters()'>?? Limpiar Filtros</button>
            <button onclick='exportLogs()'>?? Exportar</button>
        </div>
        
        <div class='logs' id='logsContainer'>
";

        output.Write(html);
    }

    private void WriteLogEntry(LogEvent logEvent, TextWriter output)
    {
        var level = logEvent.Level.ToString().ToLowerInvariant();
        var timestamp = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var message = System.Net.WebUtility.HtmlEncode(logEvent.RenderMessage());
        var levelIcon = GetLevelIcon(logEvent.Level);

        var html = new StringBuilder();
        html.AppendLine($"<div class='log-entry {level}' data-level='{level}'>");
        html.AppendLine($"  <div class='log-header'>");
        html.AppendLine($"    <span class='timestamp'>{levelIcon} {timestamp}</span>");
        html.AppendLine($"    <span class='level'>{logEvent.Level}</span>");
        html.AppendLine($"  </div>");
        html.AppendLine($"  <div class='log-body'>");
        html.AppendLine($"    <div class='log-message'>{message}</div>");

        // Propiedades
        if (logEvent.Properties.Any())
        {
            html.AppendLine($"    <div class='log-properties'>");
            html.AppendLine($"      <strong>?? Propiedades:</strong>");
            
            foreach (var property in logEvent.Properties)
            {
                var propertyName = System.Net.WebUtility.HtmlEncode(property.Key);
                var propertyValue = System.Net.WebUtility.HtmlEncode(property.Value.ToString().Trim('"'));
                
                html.AppendLine($"      <div class='property'>");
                html.AppendLine($"        <span class='property-name'>{propertyName}:</span>");
                html.AppendLine($"        <span class='property-value'>{propertyValue}</span>");
                html.AppendLine($"      </div>");
            }
            
            html.AppendLine($"    </div>");
        }

        // Excepciones
        if (logEvent.Exception != null)
        {
            var exceptionText = System.Net.WebUtility.HtmlEncode(logEvent.Exception.ToString());
            html.AppendLine($"    <div class='exception'>");
            html.AppendLine($"      <strong>?? Excepción:</strong><br><br>");
            html.AppendLine($"      {exceptionText}");
            html.AppendLine($"    </div>");
        }

        html.AppendLine($"  </div>");
        html.AppendLine($"</div>");
        html.AppendLine();

        output.Write(html.ToString());
    }

    private static string GetLevelIcon(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Error or LogEventLevel.Fatal => "?",
            LogEventLevel.Warning => "??",
            LogEventLevel.Information => "??",
            LogEventLevel.Debug => "??",
            LogEventLevel.Verbose => "??",
            _ => "??"
        };
    }

    public static void WriteHtmlFooter(TextWriter output)
    {
        var footer = @"
        </div>
        
        <div class='stats'>
            <div class='stat-item'>
                <div class='stat-value' id='totalLogs'>0</div>
                <div class='stat-label'>Total de Logs</div>
            </div>
            <div class='stat-item'>
                <div class='stat-value' id='errorCount'>0</div>
                <div class='stat-label'>Errores</div>
            </div>
            <div class='stat-item'>
                <div class='stat-value' id='warningCount'>0</div>
                <div class='stat-label'>Warnings</div>
            </div>
            <div class='stat-item'>
                <div class='stat-value' id='infoCount'>0</div>
                <div class='stat-label'>Info</div>
            </div>
        </div>
    </div>

    <script>
        function updateStats() {
            const logs = document.querySelectorAll('.log-entry');
            const errors = document.querySelectorAll('.log-entry.error').length;
            const warnings = document.querySelectorAll('.log-entry.warning').length;
            const infos = document.querySelectorAll('.log-entry.info').length;

            document.getElementById('totalLogs').textContent = logs.length;
            document.getElementById('errorCount').textContent = errors;
            document.getElementById('warningCount').textContent = warnings;
            document.getElementById('infoCount').textContent = infos;
        }

        function filterLogs() {
            const searchText = document.getElementById('searchBox').value.toLowerCase();
            const levelFilter = document.getElementById('levelFilter').value;
            const logs = document.querySelectorAll('.log-entry');

            logs.forEach(log => {
                const text = log.textContent.toLowerCase();
                const level = log.getAttribute('data-level');
                
                const matchesSearch = searchText === '' || text.includes(searchText);
                const matchesLevel = levelFilter === 'all' || level === levelFilter;

                if (matchesSearch && matchesLevel) {
                    log.classList.remove('hidden');
                } else {
                    log.classList.add('hidden');
                }
            });
        }

        function clearFilters() {
            document.getElementById('searchBox').value = '';
            document.getElementById('levelFilter').value = 'all';
            filterLogs();
        }

        function exportLogs() {
            const htmlContent = document.documentElement.outerHTML;
            const blob = new Blob([htmlContent], { type: 'text/html' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'logs_export_' + new Date().toISOString().slice(0, 10) + '.html';
            a.click();
            URL.revokeObjectURL(url);
        }

        // Actualizar estadísticas al cargar
        document.addEventListener('DOMContentLoaded', updateStats);
        
        // Auto-refresh cada 5 segundos si la página está activa
        setInterval(() => {
            if (document.visibilityState === 'visible') {
                location.reload();
            }
        }, 5000);
    </script>
</body>
</html>";

        output.Write(footer);
    }
}
