using AhorroLand.Shared.Application.Abstractions.Messaging;

namespace AhorroLand.Application.Features.Auth.Commands.UploadAvatar;

public record UploadAvatarCommand(
    Guid UserId,
    Stream FileStream,
    string FileName,
    string ContentType
) : ICommand<string>;