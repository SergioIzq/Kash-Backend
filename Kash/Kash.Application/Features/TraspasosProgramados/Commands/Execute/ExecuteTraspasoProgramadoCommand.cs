using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.TraspasosProgramados.Commands.Execute;

/// <summary>
/// Comando que Hangfire ejecutará para crear un Traspaso real desde un TraspasoProgramado.
/// Este comando es la unión entre la programación (Hangfire) y la lógica de negocio (CQRS).
/// </summary>
public sealed record ExecuteTraspasoProgramadoCommand(Guid TraspasoProgramadoId) : ICommand;
