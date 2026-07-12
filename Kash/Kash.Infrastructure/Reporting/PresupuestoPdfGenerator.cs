using System.Globalization;
using Kash.Application.Interfaces;
using Kash.Shared.Application.Dtos.Reportes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Kash.Infrastructure.Reporting;

/// <summary>
/// Genera el reporte de presupuesto financiero como PDF usando QuestPDF, con la forma de una
/// plantilla de presupuesto formal: cabecera con período, KPIs, ingresos y gastos por
/// categoría → concepto y desgloses por cuenta y forma de pago.
/// </summary>
public sealed class PresupuestoPdfGenerator : IPresupuestoPdfGenerator
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-ES");

    private static readonly Color ColorIngreso = Colors.Green.Darken2;
    private static readonly Color ColorGasto = Colors.Red.Darken2;
    private static readonly Color ColorPrimario = Colors.Blue.Darken3;
    private static readonly Color ColorCabeceraTabla = Colors.Grey.Lighten2;
    private static readonly Color ColorSubtotal = Colors.Grey.Lighten3;
    private static readonly Color ColorBorde = Colors.Grey.Lighten1;

    // Logo de Kash embebido en el ensamblado (ver EmbeddedResource en el .csproj).
    private static readonly byte[]? LogoBytes = CargarLogo();

    public byte[] Generar(PresupuestoReportDto datos)
    {
        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken4));

                page.Header().Element(c => ComponerCabecera(c, datos));
                page.Content().Element(c => ComponerContenido(c, datos));
                page.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    private static void ComponerCabecera(IContainer container, PresupuestoReportDto datos)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (LogoBytes is not null)
                    row.ConstantItem(52).PaddingRight(12).AlignMiddle()
                        .Background(Colors.White)
                        .Padding(5).Image(LogoBytes);

                row.RelativeItem().Column(titulo =>
                {
                    if (EsAnioCompleto(datos))
                    {
                        // Portada para un año natural completo: el año en grande en lugar del rango.
                        titulo.Item().Text($"Presupuesto {datos.FechaInicio.Year}")
                            .FontSize(26).Bold().FontColor(ColorPrimario);
                        titulo.Item().PaddingTop(1).Text("Resumen anual")
                            .FontSize(11).FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        titulo.Item().Text("Presupuesto Financiero")
                            .FontSize(20).Bold().FontColor(ColorPrimario);
                        titulo.Item().PaddingTop(2).Text(text =>
                        {
                            text.Span("Período: ").SemiBold();
                            text.Span($"{datos.FechaInicio:dd/MM/yyyy} — {datos.FechaFin:dd/MM/yyyy}");
                        });
                    }
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(1).LineColor(ColorPrimario);
        });
    }

    /// <summary>
    /// Carga una sola vez el logo embebido (ya en verde sobre transparente).
    /// Devuelve null si no está disponible.
    /// </summary>
    private static byte[]? CargarLogo()
    {
        var assembly = typeof(PresupuestoPdfGenerator).Assembly;
        var recurso = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("kash-logo.png", StringComparison.OrdinalIgnoreCase));

        if (recurso is null)
            return null;

        using var stream = assembly.GetManifestResourceStream(recurso);
        if (stream is null)
            return null;

        using var memoria = new MemoryStream();
        stream.CopyTo(memoria);
        return memoria.ToArray();
    }

    /// <summary>El período abarca exactamente un año natural (1 de enero – 31 de diciembre).</summary>
    private static bool EsAnioCompleto(PresupuestoReportDto datos) =>
        datos.FechaInicio.Month == 1 && datos.FechaInicio.Day == 1 &&
        datos.FechaFin.Month == 12 && datos.FechaFin.Day == 31 &&
        datos.FechaInicio.Year == datos.FechaFin.Year;

    private static void ComponerContenido(IContainer container, PresupuestoReportDto datos)
    {
        container.PaddingVertical(12).Column(col =>
        {
            col.Spacing(18);

            col.Item().Element(c => ComponerKpis(c, datos));

            if (datos.ResumenMensual.Count >= 2)
                col.Item().Element(c => ComponerResumenMensual(c, datos.ResumenMensual));

            col.Item().Element(c => ComponerSeccionCategorias(
                c, "Ingresos por categoría", datos.Ingresos, datos.TotalIngresos, ColorIngreso, esGasto: false));

            col.Item().Element(c => ComponerSeccionCategorias(
                c, "Gastos por categoría", datos.Gastos, datos.TotalGastos, ColorGasto, esGasto: true));

            col.Item().Element(c => ComponerDesgloseCuentas(c, datos.Cuentas));

            col.Item().Element(c => ComponerDesgloseFormasPago(c, datos.FormasPago));
        });
    }

    private static void ComponerKpis(IContainer container, PresupuestoReportDto datos)
    {
        var balancePositivo = datos.Balance >= 0;
        var colorBalance = balancePositivo ? ColorIngreso : ColorGasto;

        container.Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem().Element(c => CajaKpi(c, "Total ingresos", Moneda(datos.TotalIngresos), ColorIngreso));
            row.RelativeItem().Element(c => CajaKpi(c, "Total gastos", Moneda(datos.TotalGastos), ColorGasto));
            row.RelativeItem().Element(c => CajaKpi(c, balancePositivo ? "Ahorro" : "Déficit", Moneda(datos.Balance), colorBalance));
            row.RelativeItem().Element(c => CajaKpi(c, "% de ahorro", $"{datos.PorcentajeAhorro.ToString("N1", Cultura)} %", colorBalance));
        });
    }

    private static void CajaKpi(IContainer container, string titulo, string valor, Color color)
    {
        container
            .Border(1).BorderColor(ColorBorde)
            .Background(Colors.White)
            .Padding(10)
            .Column(col =>
            {
                col.Item().Text(titulo.ToUpper(Cultura)).FontSize(7.5f).FontColor(Colors.Grey.Darken1).LetterSpacing(0.03f);
                col.Item().PaddingTop(4).Text(valor).FontSize(13).Bold().FontColor(color);
            });
    }

    private static void ComponerResumenMensual(IContainer container, IReadOnlyList<ResumenMensualDto> meses)
    {
        // Escala común para las barras: el mayor importe mensual (ingreso o gasto) del período.
        var maximo = meses.Count == 0 ? 0m : meses.Max(m => Math.Max(m.Ingresos, m.Gastos));

        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Evolución mensual", ColorPrimario));

            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(78);  // Mes
                    columns.RelativeColumn();     // Barras ingresos/gastos
                    columns.ConstantColumn(78);   // Ingresos
                    columns.ConstantColumn(78);   // Gastos
                    columns.ConstantColumn(78);   // Balance
                });

                table.Header(header =>
                {
                    header.Cell().Element(CeldaCabecera).Text("Mes");
                    header.Cell().Element(CeldaCabecera).Text(string.Empty);
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Ingresos");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Gastos");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Balance");
                });

                foreach (var mes in meses)
                {
                    var nombreMes = new DateTime(mes.Anio, mes.Mes, 1).ToString("MMM yyyy", Cultura);

                    table.Cell().Element(CeldaConcepto).Text(nombreMes).SemiBold();
                    table.Cell().Element(CeldaConcepto).PaddingVertical(4).PaddingRight(8).Column(barras =>
                    {
                        BarraProporcional(barras.Item(), mes.Ingresos, maximo, ColorIngreso);
                        BarraProporcional(barras.Item().PaddingTop(2), mes.Gastos, maximo, ColorGasto);
                    });
                    table.Cell().Element(CeldaConcepto).AlignRight().Text(Moneda(mes.Ingresos)).FontColor(ColorIngreso);
                    table.Cell().Element(CeldaConcepto).AlignRight().Text(Moneda(mes.Gastos)).FontColor(ColorGasto);
                    table.Cell().Element(CeldaConcepto).AlignRight().Text(Moneda(mes.Balance))
                        .SemiBold().FontColor(mes.Balance >= 0 ? ColorIngreso : ColorGasto);
                }

                var totalIngresos = meses.Sum(m => m.Ingresos);
                var totalGastos = meses.Sum(m => m.Gastos);
                var totalBalance = totalIngresos - totalGastos;

                table.Cell().Element(CeldaTotal).Text("TOTAL").Bold();
                table.Cell().Element(CeldaTotal).Text(string.Empty);
                table.Cell().Element(CeldaTotal).AlignRight().Text(Moneda(totalIngresos)).Bold();
                table.Cell().Element(CeldaTotal).AlignRight().Text(Moneda(totalGastos)).Bold();
                table.Cell().Element(CeldaTotal).AlignRight().Text(Moneda(totalBalance))
                    .Bold().FontColor(totalBalance >= 0 ? ColorIngreso : ColorGasto);
            });
        });
    }

    /// <summary>Barra horizontal proporcional a <paramref name="valor"/> respecto de <paramref name="maximo"/>.</summary>
    private static void BarraProporcional(IContainer container, decimal valor, decimal maximo, Color color)
    {
        var fraccion = maximo > 0 ? (float)Math.Clamp(valor / maximo, 0m, 1m) : 0f;

        container.Height(4).Background(Colors.Grey.Lighten2).Row(row =>
        {
            if (fraccion > 0f)
                row.RelativeItem(fraccion).Background(color);
            if (fraccion < 1f)
                row.RelativeItem(1f - fraccion);
        });
    }

    private static void ComponerSeccionCategorias(
        IContainer container,
        string titulo,
        IReadOnlyList<CategoriaReporteDto> categorias,
        decimal total,
        Color colorAcento,
        bool esGasto)
    {
        // Los ingresos se muestran con "+" en verde y los gastos con "-" en rojo.
        var signo = esGasto ? "-" : "+";
        var colorImporte = esGasto ? ColorGasto : ColorIngreso;
        string ConSigno(decimal valor) => $"{signo}{Moneda(valor)}";

        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, titulo, colorAcento));

            if (categorias.Count == 0)
            {
                col.Item().PaddingTop(6).Text("Sin movimientos en este período.")
                    .Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();    // Categoría / concepto / descripción del movimiento
                    columns.ConstantColumn(52);  // Fecha del movimiento
                    columns.ConstantColumn(90);  // Cuenta del movimiento
                    columns.ConstantColumn(90);  // Importe
                });

                table.Header(header =>
                {
                    header.Cell().Element(CeldaCabecera).Text("Categoría / Concepto / Movimiento");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Fecha");
                    header.Cell().Element(CeldaCabecera).Text("Cuenta");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Importe");
                });

                foreach (var categoria in categorias)
                {
                    var porcentaje = total > 0 ? categoria.Total / total * 100 : 0;

                    // Fila de categoría (subtotal). El % del total va junto al nombre, no en la columna Fecha.
                    table.Cell().Element(CeldaSubtotal).Text(text =>
                    {
                        text.Span(categoria.Categoria).SemiBold();
                        text.Span($"   {porcentaje.ToString("N1", Cultura)} % del total")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().Element(CeldaSubtotal).Text(string.Empty);
                    table.Cell().Element(CeldaSubtotal).Text(string.Empty);
                    table.Cell().Element(CeldaSubtotal).AlignRight().Text(ConSigno(categoria.Total)).SemiBold().FontColor(colorImporte);

                    foreach (var concepto in categoria.Conceptos)
                    {
                        // Fila de concepto (subtotal)
                        table.Cell().Element(CeldaConcepto).PaddingLeft(12)
                            .Text($"{concepto.Concepto}  (×{concepto.Cantidad})").SemiBold();
                        table.Cell().Element(CeldaConcepto).Text(string.Empty);
                        table.Cell().Element(CeldaConcepto).Text(string.Empty);
                        table.Cell().Element(CeldaConcepto).AlignRight().Text(ConSigno(concepto.Total)).SemiBold().FontColor(colorImporte);

                        // Filas de detalle: cada movimiento individual
                        foreach (var movimiento in concepto.Movimientos)
                        {
                            var descripcion = string.IsNullOrWhiteSpace(movimiento.Descripcion)
                                ? "(sin descripción)"
                                : movimiento.Descripcion;

                            table.Cell().Element(CeldaMovimiento).PaddingLeft(24)
                                .Text(descripcion).FontSize(8).FontColor(Colors.Grey.Darken2);
                            table.Cell().Element(CeldaMovimiento).AlignRight()
                                .Text($"{movimiento.Fecha:dd/MM/yy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            table.Cell().Element(CeldaMovimiento)
                                .Text(movimiento.Cuenta).FontSize(8).FontColor(Colors.Grey.Darken1);
                            table.Cell().Element(CeldaMovimiento).AlignRight()
                                .Text(ConSigno(movimiento.Importe)).FontSize(8).FontColor(colorImporte);
                        }
                    }
                }

                table.Cell().Element(CeldaTotal).Text("TOTAL").Bold();
                table.Cell().Element(CeldaTotal).Text(string.Empty);
                table.Cell().Element(CeldaTotal).Text(string.Empty);
                table.Cell().Element(CeldaTotal).AlignRight().Text(ConSigno(total)).Bold().FontColor(colorImporte);
            });
        });
    }

    private static void ComponerDesgloseCuentas(IContainer container, IReadOnlyList<CuentaReporteDto> cuentas)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Resumen por cuenta", ColorPrimario));

            if (cuentas.Count == 0)
            {
                col.Item().PaddingTop(6).Text("Sin movimientos en este período.")
                    .Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(95);
                    columns.ConstantColumn(95);
                    columns.ConstantColumn(95);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CeldaCabecera).Text("Cuenta");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Ingresos");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Gastos");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Neto");
                });

                foreach (var cuenta in cuentas)
                {
                    table.Cell().Element(CeldaConcepto).Text(cuenta.Cuenta);
                    table.Cell().Element(CeldaConcepto).AlignRight().Text(Moneda(cuenta.Ingresos)).FontColor(ColorIngreso);
                    table.Cell().Element(CeldaConcepto).AlignRight().Text(Moneda(cuenta.Gastos)).FontColor(ColorGasto);
                    table.Cell().Element(CeldaConcepto).AlignRight().Text(Moneda(cuenta.Neto))
                        .SemiBold().FontColor(cuenta.Neto >= 0 ? ColorIngreso : ColorGasto);
                }

                table.Cell().Element(CeldaTotal).Text("TOTAL").Bold();
                table.Cell().Element(CeldaTotal).AlignRight().Text(Moneda(cuentas.Sum(c => c.Ingresos))).Bold();
                table.Cell().Element(CeldaTotal).AlignRight().Text(Moneda(cuentas.Sum(c => c.Gastos))).Bold();
                table.Cell().Element(CeldaTotal).AlignRight().Text(Moneda(cuentas.Sum(c => c.Neto))).Bold();
            });
        });
    }

    private static void ComponerDesgloseFormasPago(IContainer container, IReadOnlyList<FormaPagoReporteDto> formasPago)
    {
        container.Column(col =>
        {
            col.Item().Element(c => TituloSeccion(c, "Resumen por forma de pago", ColorPrimario));

            if (formasPago.Count == 0)
            {
                col.Item().PaddingTop(6).Text("Sin movimientos en este período.")
                    .Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(110);
                    columns.ConstantColumn(110);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CeldaCabecera).Text("Forma de pago");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Ingresos");
                    header.Cell().Element(CeldaCabecera).AlignRight().Text("Gastos");
                });

                foreach (var forma in formasPago)
                {
                    table.Cell().Element(CeldaConcepto).Text(forma.FormaPago);
                    table.Cell().Element(CeldaConcepto).AlignRight().Text(Moneda(forma.Ingresos)).FontColor(ColorIngreso);
                    table.Cell().Element(CeldaConcepto).AlignRight().Text(Moneda(forma.Gastos)).FontColor(ColorGasto);
                }

                table.Cell().Element(CeldaTotal).Text("TOTAL").Bold();
                table.Cell().Element(CeldaTotal).AlignRight().Text(Moneda(formasPago.Sum(f => f.Ingresos))).Bold();
                table.Cell().Element(CeldaTotal).AlignRight().Text(Moneda(formasPago.Sum(f => f.Gastos))).Bold();
            });
        });
    }

    private static void TituloSeccion(IContainer container, string titulo, Color color)
    {
        container
            .BorderBottom(2).BorderColor(color)
            .PaddingBottom(3)
            .Text(titulo).FontSize(12).Bold().FontColor(color);
    }

    // --- Estilos de celda reutilizables ---

    private static IContainer CeldaCabecera(IContainer container) => container
        .Background(ColorCabeceraTabla).PaddingVertical(4).PaddingHorizontal(6)
        .DefaultTextStyle(x => x.SemiBold().FontSize(8.5f));

    private static IContainer CeldaSubtotal(IContainer container) => container
        .Background(ColorSubtotal).BorderBottom(1).BorderColor(ColorBorde)
        .PaddingVertical(3).PaddingHorizontal(6);

    private static IContainer CeldaConcepto(IContainer container) => container
        .BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
        .PaddingVertical(3).PaddingHorizontal(6);

    private static IContainer CeldaMovimiento(IContainer container) => container
        .PaddingVertical(1).PaddingHorizontal(6);

    private static IContainer CeldaTotal(IContainer container) => container
        .BorderTop(2).BorderColor(ColorBorde)
        .PaddingVertical(4).PaddingHorizontal(6);

    private static void ComponerPie(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm", Cultura)} · Kash")
                    .FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(7.5f).FontColor(Colors.Grey.Darken1));
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        });
    }

    private static string Moneda(decimal valor) => valor.ToString("C2", Cultura);
}
