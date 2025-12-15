namespace Kash.Shared.Application.Dtos;

/// <summary>
/// DTO con el resumen financiero del dashboard del usuario.
/// </summary>
public sealed record DashboardResumenDto
{
    /// <summary>
    /// Balance actual del usuario (suma de saldos de todas las cuentas).
    /// </summary>
    public decimal BalanceTotal { get; init; }

    /// <summary>
    /// Total de ingresos en el período actual (mes actual).
    /// </summary>
    public decimal IngresosMesActual { get; init; }

    /// <summary>
    /// Total de gastos en el período actual (mes actual).
    /// </summary>
    public decimal GastosMesActual { get; init; }

    /// <summary>
    /// Balance del mes actual (Ingresos - Gastos).
    /// </summary>
    public decimal BalanceMesActual { get; init; }

    /// <summary>
    /// Número total de cuentas activas del usuario.
    /// </summary>
    public int TotalCuentas { get; init; }

    /// <summary>
    /// Resumen de saldos por cuenta.
    /// </summary>
    public List<CuentaResumenDto> Cuentas { get; init; } = new();

    /// <summary>
    /// Top 5 categorías con más gastos en el mes actual.
    /// </summary>
    public List<CategoriaGastoDto> TopCategoriasGastos { get; init; } = new();

    /// <summary>
    /// Últimos movimientos (combinación de ingresos y gastos).
    /// </summary>
    public List<MovimientoResumenDto> UltimosMovimientos { get; init; } = new();

    /// <summary>
    /// Comparativa con el mes anterior.
    /// </summary>
    public ComparativaMensualDto ComparativaMesAnterior { get; init; } = new();

    // ? NUEVAS MÉTRICAS

    /// <summary>
    /// Gasto promedio diario del mes actual.
    /// </summary>
    public decimal GastoPromedioDiario { get; init; }

    /// <summary>
    /// Proyección de gastos al final del mes basada en el promedio diario.
    /// </summary>
    public decimal ProyeccionGastosFinMes { get; init; }

    /// <summary>
    /// Días transcurridos del mes actual.
    /// </summary>
    public int DiasTranscurridosMes { get; init; }

    /// <summary>
    /// Días restantes del mes actual.
    /// </summary>
    public int DiasRestantesMes { get; init; }

    /// <summary>
    /// Histórico de los últimos 6 meses (ingresos y gastos).
    /// </summary>
    public List<HistoricoMensualDto> HistoricoUltimos6Meses { get; init; } = new();

    /// <summary>
    /// Presupuesto necesario calculado por cada año registrado.
    /// </summary>
    public List<PresupuestoAnualDto> PresupuestoAnual { get; init; } = new();

    /// <summary>
    /// Alertas y notificaciones para el usuario.
    /// </summary>
    public List<AlertaDto> Alertas { get; init; } = new();
}

/// <summary>
/// Resumen de una cuenta.
/// </summary>
public sealed record CuentaResumenDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public decimal Saldo { get; init; }
}

/// <summary>
/// Resumen de gastos por categoría.
/// </summary>
public sealed record CategoriaGastoDto
{
    public Guid CategoriaId { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
    public decimal TotalGastado { get; init; }
    public int CantidadTransacciones { get; init; }
    public decimal PorcentajeDelTotal { get; init; }
}

/// <summary>
/// Resumen de un movimiento (ingreso o gasto).
/// </summary>
public sealed record MovimientoResumenDto
{
    public Guid Id { get; init; }
    public string Tipo { get; init; } = string.Empty; // "Ingreso" o "Gasto"
    public decimal Importe { get; init; }
    public DateTime Fecha { get; init; }
    public string Concepto { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public string Cuenta { get; init; } = string.Empty;
}

/// <summary>
/// Comparativa entre mes actual y mes anterior.
/// </summary>
public sealed record ComparativaMensualDto
{
    public decimal IngresosMesAnterior { get; init; }
    public decimal GastosMesAnterior { get; init; }
    public decimal DiferenciaIngresos { get; init; }
    public decimal DiferenciaGastos { get; init; }
    public decimal PorcentajeCambioIngresos { get; init; }
    public decimal PorcentajeCambioGastos { get; init; }
}

// ? NUEVOS DTOs

/// <summary>
/// Datos históricos de un mes específico.
/// </summary>
public sealed record HistoricoMensualDto
{
    public int Anio { get; init; }
    public int Mes { get; init; }
    public string MesNombre { get; init; } = string.Empty;
    public decimal TotalIngresos { get; init; }
    public decimal TotalGastos { get; init; }
    public decimal Balance { get; init; }
}

/// <summary>
/// Alerta o notificación para el usuario.
/// </summary>
public sealed record AlertaDto
{
    public string Tipo { get; init; } = string.Empty; // "info", "warning", "danger", "success"
    public string Titulo { get; init; } = string.Empty;
    public string Mensaje { get; init; } = string.Empty;
    public string? Icono { get; init; }
}

/// <summary>
/// Proyección de presupuesto necesario basado en gastos históricos anuales.
/// </summary>
public sealed record PresupuestoAnualDto
{
    /// <summary>
    /// Año del registro histórico.
    /// </summary>
    public int Anio { get; init; }

    /// <summary>
    /// Total gastado acumulado durante todo el año.
    /// </summary>
    public decimal GastoTotalAnual { get; init; }

    /// <summary>
    /// Cantidad de meses que tuvieron actividad registrada ese año.
    /// (Útil para calcular promedios reales en años incompletos, como el actual).
    /// </summary>
    public int MesesRegistrados { get; init; }

    /// <summary>
    /// El "Costo de Vida" mensual promedio calculado para ese año.
    /// Fórmula: GastoTotalAnual / MesesRegistrados.
    /// </summary>
    public decimal PromedioMensualNecesario { get; init; }
}