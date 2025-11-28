using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Application.Features.Auth.Commands.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailCommand(string Correo) : ICommand;