using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.TraspasosProgramados.Commands;

public sealed record CreateTraspasoProgramadoCommand : AbsCreateCommand<TraspasoProgramado, TraspasoProgramadoId>
{
    public required Guid CuentaOrigenId { get; init; }
    public required Guid CuentaDestinoId { get; init; }
    public required decimal Importe { get; init; }
    public required DateTime FechaEjecucion { get; init; }
    public required string Frecuencia { get; init; }
    public required Guid UsuarioId { get; init; }
    public string? Descripcion { get; init; }
    public bool Activo { get; init; }
}
