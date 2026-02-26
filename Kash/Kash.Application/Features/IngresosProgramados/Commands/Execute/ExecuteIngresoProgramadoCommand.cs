using Kash.Shared.Application.Abstractions.Messaging;

namespace Kash.Application.Features.IngresosProgramados.Commands.Execute;

/// <summary>
/// Comando que Hangfire ejecutará para crear un Ingreso real desde un IngresoProgramado.
/// Este comando es la unión entre la programación (Hangfire) y la lógica de negocio (CQRS).
/// </summary>
public sealed record ExecuteIngresoProgramadoCommand(Guid hangfireId) : ICommand;
