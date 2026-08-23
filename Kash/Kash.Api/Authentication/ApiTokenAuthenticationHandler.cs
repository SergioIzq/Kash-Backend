using System.Security.Claims;
using System.Text.Encodings.Web;
using Kash.Domain;
using Kash.Shared.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Kash.Api.Authentication;

public sealed class ApiTokenAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

/// <summary>
/// Autentica peticiones con el token de API personal del usuario (ver token-personal-api).
/// Emite los mismos claims (sub, email, NameIdentifier) que <c>KernelJwtTokenGenerator</c> para
/// que <c>AbsController.GetCurrentUserId()/RequireCurrentUserId()</c> funcionen sin cambios.
/// </summary>
public sealed class ApiTokenAuthenticationHandler : AuthenticationHandler<ApiTokenAuthenticationSchemeOptions>
{
    private const string BearerPrefix = "Bearer ";

    private readonly IUsuarioReadRepository _usuarioReadRepository;
    private readonly IApiTokenHasher _apiTokenHasher;

    public ApiTokenAuthenticationHandler(
        IOptionsMonitor<ApiTokenAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUsuarioReadRepository usuarioReadRepository,
        IApiTokenHasher apiTokenHasher)
        : base(options, logger, encoder)
    {
        _usuarioReadRepository = usuarioReadRepository;
        _apiTokenHasher = apiTokenHasher;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader[BearerPrefix.Length..].Trim();

        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        var hash = _apiTokenHasher.Hash(token);
        var usuario = await _usuarioReadRepository.GetByApiTokenHashAsync(hash, Context.RequestAborted);

        if (usuario is null)
        {
            return AuthenticateResult.Fail("Token de API inválido.");
        }

        var claims = new[]
        {
            new Claim("sub", usuario.Id.Value.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.Value.ToString()),
            new Claim("email", usuario.Correo.Value)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
