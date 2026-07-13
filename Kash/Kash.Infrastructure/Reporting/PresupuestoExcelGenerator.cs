using ClosedXML.Excel;
using Kash.Application.Interfaces;
using Kash.Shared.Application.Dtos.Reportes;

namespace Kash.Infrastructure.Reporting;

/// <summary>
/// Genera el reporte de presupuesto financiero como libro Excel (.xlsx) usando ClosedXML,
/// con una hoja de resumen (KPIs + evolución mensual) y una hoja por sección: ingresos,
/// gastos, cuentas y formas de pago. Mismo esquema de colores que el PDF.
/// </summary>
public sealed class PresupuestoExcelGenerator : IPresupuestoExcelGenerator
{
    private static readonly XLColor ColorIngreso = XLColor.FromHtml("#2E7D32");
    private static readonly XLColor ColorGasto = XLColor.FromHtml("#C62828");
    private static readonly XLColor ColorPrimario = XLColor.FromHtml("#1565C0");
    private static readonly XLColor ColorCabeceraTabla = XLColor.FromHtml("#1565C0");
    private static readonly XLColor ColorFilaAlterna = XLColor.FromHtml("#F2F6FC");

    private const string FormatoMoneda = "#,##0.00 €";
    private const string FormatoFecha = "dd/mm/yyyy";

    public byte[] Generar(PresupuestoReportDto datos)
    {
        using var libro = new XLWorkbook();

        ComponerResumen(libro, datos);
        ComponerMovimientos(libro, "Ingresos", datos.Ingresos, ColorIngreso);
        ComponerMovimientos(libro, "Gastos", datos.Gastos, ColorGasto);
        ComponerCuentas(libro, datos.Cuentas);
        ComponerFormasPago(libro, datos.FormasPago);

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static void ComponerResumen(XLWorkbook libro, PresupuestoReportDto datos)
    {
        var ws = libro.Worksheets.Add("Resumen");
        ws.ShowGridLines = false;

        ws.Cell(1, 1).Value = "PRESUPUESTO FINANCIERO";
        ws.Range(1, 1, 1, 4).Merge().Style
            .Font.SetBold().Font.SetFontSize(16).Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(ColorPrimario)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws.Row(1).Height = 28;

        ws.Cell(2, 1).Value = "Período";
        ws.Cell(2, 1).Style.Font.SetItalic().Font.SetFontColor(XLColor.FromHtml("#757575"));

        ws.Cell(2, 2).Value = $"{datos.FechaInicio:dd/MM/yyyy} - {datos.FechaFin:dd/MM/yyyy}";
        ws.Range(2, 2, 2, 4).Merge();
        ws.Cell(2, 2).Style.Font.SetItalic().Font.SetFontColor(XLColor.FromHtml("#757575"));

        var kpis = new (string Etiqueta, decimal Valor, XLColor? Color)[]
        {
            ("Total ingresos", datos.TotalIngresos, ColorIngreso),
            ("Total gastos", datos.TotalGastos, ColorGasto),
            ("Balance", datos.Balance, datos.Balance >= 0 ? ColorIngreso : ColorGasto),
        };

        var fila = 4;
        foreach (var (etiqueta, valor, color) in kpis)
        {
            ws.Cell(fila, 1).Value = etiqueta;
            ws.Cell(fila, 1).Style.Font.SetBold();
            ws.Cell(fila, 2).Value = valor;
            ws.Cell(fila, 2).Style.NumberFormat.Format = FormatoMoneda;
            ws.Cell(fila, 2).Style.Font.SetBold().Font.SetFontColor(color);
            fila++;
        }

        ws.Cell(fila, 1).Value = "% Ahorro";
        ws.Cell(fila, 1).Style.Font.SetBold();
        ws.Cell(fila, 2).Value = datos.PorcentajeAhorro / 100m;
        ws.Cell(fila, 2).Style.NumberFormat.Format = "0.0%";
        ws.Cell(fila, 2).Style.Font.SetBold();
        fila += 2;

        if (datos.ResumenMensual.Count > 0)
        {
            var headers = new[] { "Año", "Mes", "Ingresos", "Gastos", "Balance" };
            EscribirCabeceraTabla(ws, fila, headers, ColorPrimario);
            fila++;

            var filaInicio = fila;
            foreach (var mes in datos.ResumenMensual)
            {
                ws.Cell(fila, 1).Value = mes.Anio;
                ws.Cell(fila, 2).Value = mes.Mes;
                ws.Cell(fila, 3).Value = mes.Ingresos;
                ws.Cell(fila, 4).Value = mes.Gastos;
                ws.Cell(fila, 5).Value = mes.Balance;
                fila++;
            }

            var rango = ws.Range(filaInicio, 1, fila - 1, 5);
            EstilizarTabla(rango, filaInicio, fila - 1, columnasMoneda: [3, 4, 5]);
        }

        ws.Columns(1, 5).AdjustToContents();
        ws.Column(1).Width = Math.Max(ws.Column(1).Width, 16);
    }

    private static void ComponerMovimientos(
        XLWorkbook libro, string nombreHoja, IReadOnlyList<CategoriaReporteDto> categorias, XLColor colorCabecera)
    {
        var ws = libro.Worksheets.Add(nombreHoja);
        ws.ShowGridLines = false;

        var headers = new[] { "Categoría", "Concepto", "Fecha", "Descripción", "Cuenta", "Importe" };
        EscribirCabeceraTabla(ws, 1, headers, colorCabecera);

        var fila = 2;
        foreach (var categoria in categorias)
        {
            foreach (var concepto in categoria.Conceptos)
            {
                foreach (var movimiento in concepto.Movimientos)
                {
                    ws.Cell(fila, 1).Value = categoria.Categoria;
                    ws.Cell(fila, 2).Value = concepto.Concepto;
                    ws.Cell(fila, 3).Value = movimiento.Fecha;
                    ws.Cell(fila, 3).Style.DateFormat.Format = FormatoFecha;
                    ws.Cell(fila, 4).Value = movimiento.Descripcion;
                    ws.Cell(fila, 5).Value = movimiento.Cuenta;
                    ws.Cell(fila, 6).Value = movimiento.Importe;
                    fila++;
                }
            }
        }

        if (fila > 2)
        {
            var rango = ws.Range(1, 1, fila - 1, 6);
            EstilizarTabla(rango, 2, fila - 1, columnasMoneda: [6]);
        }

        ws.SheetView.Freeze(1, 0);
        ws.Columns(1, 6).AdjustToContents();
    }

    private static void ComponerCuentas(XLWorkbook libro, IReadOnlyList<CuentaReporteDto> cuentas)
    {
        var ws = libro.Worksheets.Add("Cuentas");
        ws.ShowGridLines = false;

        var headers = new[] { "Cuenta", "Ingresos", "Gastos", "Neto" };
        EscribirCabeceraTabla(ws, 1, headers, ColorPrimario);

        var fila = 2;
        foreach (var cuenta in cuentas)
        {
            ws.Cell(fila, 1).Value = cuenta.Cuenta;
            ws.Cell(fila, 2).Value = cuenta.Ingresos;
            ws.Cell(fila, 3).Value = cuenta.Gastos;
            ws.Cell(fila, 4).Value = cuenta.Neto;
            fila++;
        }

        if (fila > 2)
        {
            var rango = ws.Range(1, 1, fila - 1, 4);
            EstilizarTabla(rango, 2, fila - 1, columnasMoneda: [2, 3, 4]);
        }

        ws.Columns(1, 4).AdjustToContents();
    }

    private static void ComponerFormasPago(XLWorkbook libro, IReadOnlyList<FormaPagoReporteDto> formasPago)
    {
        var ws = libro.Worksheets.Add("Formas de pago");
        ws.ShowGridLines = false;

        var headers = new[] { "Forma de pago", "Ingresos", "Gastos" };
        EscribirCabeceraTabla(ws, 1, headers, ColorPrimario);

        var fila = 2;
        foreach (var forma in formasPago)
        {
            ws.Cell(fila, 1).Value = forma.FormaPago;
            ws.Cell(fila, 2).Value = forma.Ingresos;
            ws.Cell(fila, 3).Value = forma.Gastos;
            fila++;
        }

        if (fila > 2)
        {
            var rango = ws.Range(1, 1, fila - 1, 3);
            EstilizarTabla(rango, 2, fila - 1, columnasMoneda: [2, 3]);
        }

        ws.Columns(1, 3).AdjustToContents();
    }

    private static void EscribirCabeceraTabla(IXLWorksheet ws, int fila, string[] headers, XLColor color)
    {
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(fila, i + 1).Value = headers[i];

        var rango = ws.Range(fila, 1, fila, headers.Length);
        rango.Style.Font.SetBold().Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(color)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws.Row(fila).Height = 20;
    }

    /// <summary>Bordes, filas alternas y formato moneda para el cuerpo de una tabla ya escrita.</summary>
    private static void EstilizarTabla(IXLRange rango, int filaDatosInicio, int filaDatosFin, int[] columnasMoneda)
    {
        rango.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        rango.Style.Border.SetInsideBorder(XLBorderStyleValues.Hair);

        for (var fila = filaDatosInicio; fila <= filaDatosFin; fila++)
        {
            if ((fila - filaDatosInicio) % 2 == 1)
                rango.Worksheet.Row(fila).Style.Fill.SetBackgroundColor(ColorFilaAlterna);

            foreach (var col in columnasMoneda)
                rango.Worksheet.Cell(fila, col).Style.NumberFormat.Format = FormatoMoneda;
        }
    }
}
