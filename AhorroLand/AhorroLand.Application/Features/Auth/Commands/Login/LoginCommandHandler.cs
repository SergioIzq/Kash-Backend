using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Abstractions.Errors;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AhorroLand.Application.Features.Auth.Commands.Login;

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
            // SEGURIDAD: Decimos "Credenciales inválidas" para no revelar que el email no existe.
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        // 4. Verificar contraseña
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Contrasena, usuario.ContrasenaHash.Value);

        if (!isPasswordValid)
        {
            // Mismo error que arriba. El atacante no sabe si falló el email o el pass.
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

