using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Traspasos.Commands;

public sealed record CreateTraspasoCommand : AbsCreateCommand<Traspaso, TraspasoId>
{
    public required Guid CuentaOrigenId { get; init; }
    public required Guid CuentaDestinoId { get; init; }
    public required Guid UsuarioId { get; init; }
    public required decimal Importe { get; init; }
    public required DateTime Fecha { get; init; }
    public string? Descripcion { get; init; }
}
