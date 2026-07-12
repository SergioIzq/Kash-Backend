using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Correo,
    string Contrasena
) : ICommand<LoginResponse>;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAt
);
