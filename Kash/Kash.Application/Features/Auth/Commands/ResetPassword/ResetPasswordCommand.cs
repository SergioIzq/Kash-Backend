using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : ICommand;
