namespace Kash.Application.Features.Inversiones.Commands.Import.Models;

public sealed record ImportarExtractoResult(
    int Importadas,
    int Duplicadas,
    List<ImportErrorLinea> Errores);
