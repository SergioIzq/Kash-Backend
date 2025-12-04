using AhorroLand.Shared.Application.Abstractions.Messaging;

namespace AhorroLand.Application.Features.Auth.Commands.UpdateUserProfile
{
    public record UpdateUserProfileCommand(
        Guid UserId,
        string Nombre,
        string? Apellidos
    ) : ICommand;
}
