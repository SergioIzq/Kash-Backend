using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using Kash.Shared.Domain.Abstractions.Errors;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Kash.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUsuarioReadRepository _usuarioReadRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUsuarioReadRepository usuarioReadRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginCommandHandler> logger
        )
    {
        _usuarioReadRepository = usuarioReadRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailVO = Email.Create(request.Correo).Value;
        var usuario = await _usuarioReadRepository.GetByEmailAsync(emailVO, cancellationToken);

        if (usuario is null)
        {
            // SEGURIDAD: Decimos "Credenciales inv�lidas" para no revelar que el email no existe.
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        // 4. Verificar contrase�a
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Contrasena, usuario.ContrasenaHash.Value);

        if (!isPasswordValid)
        {
            // Mismo error que arriba. El atacante no sabe si fallel email o el pass.
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        if (!usuario.Activo)
        {
            return Result.Failure<LoginResponse>(UsuarioErrors.DeactivatedAccount);
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(usuario);

        return Result.Success(new LoginResponse(token, expiresAt));
    }
}

