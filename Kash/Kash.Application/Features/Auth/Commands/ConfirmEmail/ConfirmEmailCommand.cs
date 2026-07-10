using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.Auth.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(
    string Token
) : ICommand<ConfirmEmailResponse>;

public sealed record ConfirmEmailResponse(
    string Mensaje
);
