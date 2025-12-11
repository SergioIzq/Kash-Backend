using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Traspasos.Commands;

public sealed record CreateTraspasoCommand : AbsCreateCommand<Traspaso, TraspasoId>
{
    public required Guid CuentaOrigenId { get; init; }
    public required Guid CuentaDestinoId { get; init; }
    public required Guid UsuarioId { get; init; }
    public required decimal Importe { get; init; }
    public required DateTime Fecha { get; init; }
    public string? Descripcion { get; init; }
    public bool Activo { get; init; }
}
