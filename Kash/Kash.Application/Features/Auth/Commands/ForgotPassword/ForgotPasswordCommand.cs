using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand;
