using Serilog.Sinks.File;
using System.Text;

namespace Kash.FicheroLog.Formatters;

/// <summary>
/// Hooks del ciclo de vida del archivo de log HTML.
/// NOTA: El archivo HTML no se cierra explícitamente porque Serilog no provee un hook para eso.
/// Los navegadores modernos tolerarán el HTML sin etiquetas de cierre al final.
/// </summary>
public class HtmlLogHooks : FileLifecycleHooks
{
    private readonly string _pageTitle;

    public HtmlLogHooks(string pageTitle = "Logs del Sistema")
    {
        _pageTitle = pageTitle;
    }

    public override Stream OnFileOpened(Stream fileStream, Encoding streamEncoding)
    {
        // Solo escribimos la estructura inicial si el archivo está vacío
        if (fileStream.Length == 0)
        {
            using var writer = new StreamWriter(fileStream, streamEncoding, leaveOpen: true);
            writer.Write(GetHtmlHeader());
            writer.Flush();
        }

        return fileStream;
    }

    private string GetHtmlHeader()
    {
        // NOTA: Usamos $@" para strings multilínea. 
        // Las llaves de CSS y JS deben escaparse duplicándolas {{ }}.
        // Las llaves de C# (variables) se usan normales {var}.

        return $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{_pageTitle}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}

        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
            padding-bottom: 140px; /* Espacio reservado para el footer fijo */
            min-height: 100vh;
        }}

        .container {{
            max-width: 1400px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            overflow: hidden;
            margin-bottom: 20px; 
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

        .header .subtitle {{ opacity: 0.9; font-size: 1.1em; }}

        .controls {{
            padding: 20px 30px;
            background: #f8f9fa;
            border-bottom: 2px solid #e9ecef;
            display: flex;
            gap: 15px;
            flex-wrap: wrap;
            align-items: center;
            position: sticky;
            top: 0;
            z-index: 100;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
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

        .logs {{ padding: 30px; min-height: 200px; }}

        .log-entry {{
            margin-bottom: 20px;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            transition: all 0.3s;
            animation: slideIn 0.3s ease-out;
            background: white;
        }}

        @keyframes slideIn {{
            from {{ opacity: 0; transform: translateY(-10px); }}
            to {{ opacity: 1; transform: translateY(0); }}
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

        .log-body {{ padding: 20px; background: white; }}
        .log-message {{ font-size: 1.1em; line-height: 1.6; color: #2c3e50; margin-bottom: 15px; font-weight: 500; }}

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

        .property-name {{ font-weight: 700; color: #667eea; }}
        .property-value {{ color: #495057; margin-left: 10px; }}

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

        .log-entry.error .log-header {{ background: linear-gradient(135deg, #fc8181 0%, #e53e3e 100%); }}
        .log-entry.warning .log-header {{ background: linear-gradient(135deg, #f6ad55 0%, #ed8936 100%); }}
        .log-entry.info .log-header {{ background: linear-gradient(135deg, #4299e1 0%, #3182ce 100%); }}
        .log-entry.debug .log-header {{ background: linear-gradient(135deg, #68d391 0%, #38b2ac 100%); }}
        .log-entry.verbose .log-header {{ background: linear-gradient(135deg, #a0aec0 0%, #718096 100%); }}

        /* --- Footer Fijo para Estadísticas --- */
        .stats-fixed-footer {{
            position: fixed;
            bottom: 0;
            left: 0;
            width: 100%;
            padding: 15px 30px;
            background: white;
            border-top: 4px solid #667eea;
            display: flex;
            justify-content: center;
            gap: 40px;
            box-shadow: 0 -5px 20px rgba(0,0,0,0.1);
            z-index: 1000;
            flex-wrap: wrap;
        }}

        .stat-item {{ text-align: center; display: flex; flex-direction: column; align-items: center; }}
        .stat-value {{ font-size: 1.8em; font-weight: bold; color: #667eea; line-height: 1; }}
        .stat-label {{ color: #6c757d; font-size: 0.85em; margin-top: 5px; text-transform: uppercase; letter-spacing: 0.5px; }}

        .hidden {{ display: none !important; }}
    </style>

    <script>
        function updateStats() {{
            const logs = document.querySelectorAll('.log-entry');
            const errors = document.querySelectorAll('.log-entry.error').length;
            const warnings = document.querySelectorAll('.log-entry.warning').length;
            const infos = document.querySelectorAll('.log-entry.info').length;

            const totalEl = document.getElementById('totalLogs');
            if(totalEl) {{
                totalEl.textContent = logs.length;
                document.getElementById('errorCount').textContent = errors;
                document.getElementById('warningCount').textContent = warnings;
                document.getElementById('infoCount').textContent = infos;
            }}
        }}

        function filterLogs() {{
            const searchText = document.getElementById('searchBox').value.toLowerCase();
            const levelFilter = document.getElementById('levelFilter').value;
            const logs = document.querySelectorAll('.log-entry');

            logs.forEach(log => {{
                const text = log.textContent.toLowerCase();
                const level = log.getAttribute('data-level');
                
                const matchesSearch = searchText === '' || text.includes(searchText);
                const matchesLevel = levelFilter === 'all' || level === levelFilter;

                if (matchesSearch && matchesLevel) {{
                    log.classList.remove('hidden');
                }} else {{
                    log.classList.add('hidden');
                }}
            }});
        }}

        function clearFilters() {{
            document.getElementById('searchBox').value = '';
            document.getElementById('levelFilter').value = 'all';
            filterLogs();
        }}

        function exportLogs() {{
            const currentHtml = document.documentElement.innerHTML;
            const fullHtml = '<!DOCTYPE html><html>' + currentHtml + '</body></html>';
            const blob = new Blob([fullHtml], {{ type: 'text/html' }});
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'logs_' + new Date().toISOString().slice(0, 19).replace(/:/g,'-') + '.html';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        }}

        document.addEventListener('DOMContentLoaded', () => {{
            updateStats();
            const searchBox = document.getElementById('searchBox');
            if(searchBox) searchBox.addEventListener('keyup', filterLogs);
        }});

        setInterval(() => {{
             updateStats();
        }}, 2000);
    </script>
</head>
<body>
    <div class='stats-fixed-footer'>
        <div class='stat-item'>
            <div class='stat-value' id='totalLogs'>0</div>
            <div class='stat-label'>Total</div>
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

    <div class='container'>
        <div class='header'>
            <h1>📜 {_pageTitle}</h1>
            <div class='subtitle'>Sistema de Monitoreo de Logs en Tiempo Real</div>
        </div>
        
        <div class='controls'>
            <input type='text' id='searchBox' placeholder='🔍 Buscar en los logs...' onkeyup='filterLogs()'>
            <select id='levelFilter' onchange='filterLogs()'>
                <option value='all'>Todos los niveles</option>
                <option value='error'>❌ Solo Errores</option>
                <option value='warning'>⚠️ Solo Warnings</option>
                <option value='info'>ℹ️ Solo Info</option>
                <option value='debug'>🐛 Solo Debug</option>
            </select>
            <button onclick='clearFilters()'>🧹 Limpiar</button>
            <button onclick='exportLogs()'>💾 Exportar</button>
            <button onclick='location.reload()'>🔄 Refrescar</button>
        </div>

        <div class='logs' id='logsContainer'>
<!-- ⚠️ Los logs se escribirán aquí. El HTML no se cierra explícitamente porque Serilog no provee un hook de cierre.
     Los navegadores modernos toleran HTML sin cerrar correctamente. -->
";
    }
}
