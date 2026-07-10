using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.Auth.Commands.UpdateUserProfile
{
    public record UpdateUserProfileCommand(
        Guid UserId,
        string Nombre,
        string? Apellidos
    ) : ICommand;
}
