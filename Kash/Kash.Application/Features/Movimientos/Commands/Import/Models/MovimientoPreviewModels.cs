namespace Kash.Application.Features.Movimientos.Commands.Import.Models;

/// <summary>
/// Una línea del extracto tal como el backend PROPONE crearla. El usuario la revisa/edita
/// antes de confirmar. <see cref="Tipo"/> es "gasto" o "ingreso".
/// </summary>
public sealed record MovimientoPreviewDto(
    string Id,
    DateTime Fecha,
    string Descripcion,
    decimal Importe,
    string Tipo,
    string CuentaNombre,
    string CategoriaNombre,
    string ConceptoNombre,
    string FormaPagoNombre,
    string? ProveedorNombre,
    bool EsDuplicado,
    bool ReglaAplicada,
    string? ReglaPatron);

/// <summary>Resultado de la previsualización: movimientos propuestos + errores de parseo.</summary>
public sealed record PreviewMovimientosResult(
    List<MovimientoPreviewDto> Movimientos,
    List<MovimientoImportError> Errores);

/// <summary>
/// Una línea ya revisada por el usuario que se enviará al backend para crearse tal cual.
/// <see cref="Tipo"/> es "gasto" o "ingreso".
/// </summary>
public sealed record MovimientoConfirmarDto(
    DateTime Fecha,
    string Descripcion,
    decimal Importe,
    string Tipo,
    string CuentaNombre,
    string CategoriaNombre,
    string ConceptoNombre,
    string FormaPagoNombre,
    string? ProveedorNombre);
