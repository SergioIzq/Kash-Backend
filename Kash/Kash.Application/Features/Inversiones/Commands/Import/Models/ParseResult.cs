namespace Kash.Application.Features.Inversiones.Commands.Import.Models;

public sealed record ParseResult(
    List<InversionImportDto> Rows,
    List<ImportErrorLinea> Errores);
