using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.Auth.Commands.UploadAvatar;

public record UploadAvatarCommand(
    Guid UserId,
    Stream FileStream,
    string FileName,
    string ContentType
) : ICommand<string>;
