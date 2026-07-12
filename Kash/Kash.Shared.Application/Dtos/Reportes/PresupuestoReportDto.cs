namespace Kash.Shared.Application.Dtos.Reportes;

/// <summary>
/// Datos completos del reporte de presupuesto financiero para un rango de fechas.
/// Estructurado como una plantilla de presupuesto formal: KPIs, ingresos y gastos
/// agrupados por categoría → concepto, y desgloses por cuenta y forma de pago.
/// </summary>
public sealed record PresupuestoReportDto
{
    public DateTime FechaInicio { get; init; }
    public DateTime FechaFin { get; init; }

    public decimal TotalIngresos { get; init; }
    public decimal TotalGastos { get; init; }

    /// <summary>Ingresos menos gastos del período (positivo = ahorro, negativo = déficit).</summary>
    public decimal Balance => TotalIngresos - TotalGastos;

    /// <summary>Porcentaje del ingreso que queda como ahorro. 0 si no hubo ingresos.</summary>
    public decimal PorcentajeAhorro => TotalIngresos > 0 ? Balance / TotalIngresos * 100 : 0;

    public IReadOnlyList<ResumenMensualDto> ResumenMensual { get; init; } = [];
    public IReadOnlyList<CategoriaReporteDto> Ingresos { get; init; } = [];
    public IReadOnlyList<CategoriaReporteDto> Gastos { get; init; } = [];
    public IReadOnlyList<CuentaReporteDto> Cuentas { get; init; } = [];
    public IReadOnlyList<FormaPagoReporteDto> FormasPago { get; init; } = [];
}

/// <summary>Ingresos, gastos y balance de un mes concreto del período.</summary>
public sealed record ResumenMensualDto
{
    public int Anio { get; init; }
    public int Mes { get; init; }
    public decimal Ingresos { get; init; }
    public decimal Gastos { get; init; }
    public decimal Balance => Ingresos - Gastos;
}

/// <summary>Una categoría con su total y el desglose por concepto.</summary>
public sealed record CategoriaReporteDto
{
    public string Categoria { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public IReadOnlyList<ConceptoReporteDto> Conceptos { get; init; } = [];
}

/// <summary>Un concepto con su total, el número de movimientos y el detalle de cada uno.</summary>
public sealed record ConceptoReporteDto
{
    public string Concepto { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public int Cantidad { get; init; }
    public IReadOnlyList<MovimientoDetalleReporteDto> Movimientos { get; init; } = [];
}

/// <summary>Un movimiento individual (gasto o ingreso) dentro de un concepto.</summary>
public sealed record MovimientoDetalleReporteDto
{
    public DateTime Fecha { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public string Cuenta { get; init; } = string.Empty;
    public decimal Importe { get; init; }
}

/// <summary>Ingresos y gastos imputados a una cuenta durante el período.</summary>
public sealed record CuentaReporteDto
{
    public string Cuenta { get; init; } = string.Empty;
    public decimal Ingresos { get; init; }
    public decimal Gastos { get; init; }
    public decimal Neto => Ingresos - Gastos;
}

/// <summary>Ingresos y gastos por forma de pago durante el período.</summary>
public sealed record FormaPagoReporteDto
{
    public string FormaPago { get; init; } = string.Empty;
    public decimal Ingresos { get; init; }
    public decimal Gastos { get; init; }
}
