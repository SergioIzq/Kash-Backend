using SergioIzq.Domain.Kernel.Abstractions.Enums;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Domain;

public static class InversionErrors
{
    public static readonly Error NotFound = new(
        "Inversion.NotFound",
        "Inversión no encontrada",
        "La inversión no existe o no te pertenece.",
        ErrorType.NotFound);

    public static readonly Error TickerRequerido = new(
        "Inversion.TickerRequerido",
        "Ticker obligatorio",
        "El ticker es obligatorio para este tipo de activo.",
        ErrorType.Validation);
}
