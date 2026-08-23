using SergioIzq.Application.Kernel.Messaging;

namespace Kash.Application.Features.Auth.Commands.GenerateApiToken;

/// <summary>
/// Genera (o regenera) el token de API personal del usuario autenticado. El valor en claro
/// devuelto solo se muestra en esta respuesta; a partir de aquí solo se persiste su hash.
/// Regenerar sustituye cualquier token anterior, invalidándolo de inmediato.
/// </summary>
public sealed record GenerateApiTokenCommand(Guid UsuarioId) : ICommand<string>;
