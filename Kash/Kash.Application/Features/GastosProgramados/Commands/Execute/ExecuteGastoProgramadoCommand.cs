using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.GastosProgramados.Commands.Execute;

/// <summary>
/// Comando que Hangfire ejecutará para crear un Gasto real desde un GastoProgramado.
/// Este comando es la unión entre la programación (Hangfire) y la lógica de negocio (CQRS).
/// </summary>
public sealed record ExecuteGastoProgramadoCommand(Guid GastoProgramadoId) : ICommand;
