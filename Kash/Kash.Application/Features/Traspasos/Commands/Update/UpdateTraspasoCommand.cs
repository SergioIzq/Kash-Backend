using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Traspasos.Commands;

public sealed record UpdateTraspasoCommand : AbsUpdateCommand<Traspaso, TraspasoId, TraspasoDto>
{
    public required Guid CuentaOrigenId { get; init; }
    public required Guid CuentaDestinoId { get; init; }
    public required decimal Importe { get; init; }
    public required DateTime FechaEjecucion { get; init; }
    public string? Descripcion { get; init; }
    public bool Activo { get; init; }
}
