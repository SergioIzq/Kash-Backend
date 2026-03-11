using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Inversiones.Commands;

public sealed record CreateInversionCommand : AbsCreateCommand<Inversion, InversionId>
{
    public required string  Nombre      { get; init; }
    public required string  Ticker      { get; init; }

    /// <summary>
    /// Tipo en formato lowercase: "etf" | "accion" | "cripto" | "bono" | "fondo" | "mercado_privado" | "otro"
    /// </summary>
    public required string  Tipo        { get; init; }

    public required decimal Cantidad    { get; init; }
    public required decimal PrecioCompra { get; init; }
    public required string  Moneda      { get; init; }
    public required DateTime FechaCompra { get; init; }
    public string?          Descripcion { get; init; }
    public string?          Plataforma  { get; init; }
}
