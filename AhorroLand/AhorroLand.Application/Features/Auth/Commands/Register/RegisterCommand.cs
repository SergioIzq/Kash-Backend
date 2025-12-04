using AhorroLand.Shared.Application.Abstractions.Messaging;

namespace AhorroLand.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Correo,
    string Contrasena,
    string Nombre,
    string Apellidos
) : ICommand;
