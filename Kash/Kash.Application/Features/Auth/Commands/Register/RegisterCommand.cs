using Kash.Shared.Application.Abstractions.Messaging;

namespace Kash.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Correo,
    string Contrasena,
    string Nombre,
    string? Apellidos
) : ICommand;
