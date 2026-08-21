using ClosedXML.Excel;
using Kash.Application.Interfaces;
using Kash.Shared.Application.Dtos;

namespace Kash.Infrastructure.Reporting;

/// <summary>
/// Genera un Excel de detalle (una fila por Ingreso) con ClosedXML, mismo esquema visual que
/// <see cref="PresupuestoExcelGenerator"/>.
/// </summary>
public sealed class IngresoExcelGenerator : IIngresoExcelGenerator
{
    private static readonly XLColor ColorCabeceraTabla = XLColor.FromHtml("#2E7D32");
    private static readonly XLColor ColorFilaAlterna = XLColor.FromHtml("#F2F6FC");

    private const string FormatoMoneda = "#,##0.00 €";
    private const string FormatoFecha = "dd/mm/yyyy";

    public byte[] Generar(IReadOnlyList<IngresoDto> datos)
    {
        using var libro = new XLWorkbook();
        var ws = libro.Worksheets.Add("Ingresos");
        ws.ShowGridLines = false;

        var headers = new[] { "Fecha", "Concepto", "Categoría", "Cliente", "Persona", "Cuenta", "Forma de Pago", "Importe", "Descripción" };
        EscribirCabeceraTabla(ws, headers);

        var fila = 2;
        foreach (var ingreso in datos)
        {
            ws.Cell(fila, 1).Value = ingreso.Fecha;
            ws.Cell(fila, 1).Style.DateFormat.Format = FormatoFecha;
            ws.Cell(fila, 2).Value = ingreso.ConceptoNombre;
            ws.Cell(fila, 3).Value = ingreso.CategoriaNombre ?? string.Empty;
            ws.Cell(fila, 4).Value = ingreso.ClienteNombre ?? string.Empty;
            ws.Cell(fila, 5).Value = ingreso.PersonaNombre ?? string.Empty;
            ws.Cell(fila, 6).Value = ingreso.CuentaNombre;
            ws.Cell(fila, 7).Value = ingreso.FormaPagoNombre;
            ws.Cell(fila, 8).Value = ingreso.Importe;
            ws.Cell(fila, 9).Value = ingreso.Descripcion ?? string.Empty;
            fila++;
        }

        if (fila > 2)
        {
            var rango = ws.Range(1, 1, fila - 1, headers.Length);
            EstilizarTabla(rango, 2, fila - 1, columnasMoneda: [8]);
        }

        ws.SheetView.Freeze(1, 0);
        ws.Columns(1, headers.Length).AdjustToContents();

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static void EscribirCabeceraTabla(IXLWorksheet ws, string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        var rango = ws.Range(1, 1, 1, headers.Length);
        rango.Style.Font.SetBold().Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(ColorCabeceraTabla)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
        ws.Row(1).Height = 20;
    }

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
