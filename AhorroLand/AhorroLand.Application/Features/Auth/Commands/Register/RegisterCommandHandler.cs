using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Abstractions.Services;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Abstractions.Errors;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand>
{
    private readonly IUsuarioWriteRepository _usuarioWriteRepository;
    private readonly IUsuarioReadRepository _usuarioReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IUsuarioWriteRepository usuarioWriteRepository,
        IUsuarioReadRepository usuarioReadRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEmailService emailService)
    {
        _usuarioWriteRepository = usuarioWriteRepository;
        _usuarioReadRepository = usuarioReadRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {

        // 1. Validar que el correo no exista
        var emailResult = Email.Create(request.Correo);
        var existingUser = await _usuarioReadRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure(AuthErrors.InvalidCredentials);
        }

        // 2. Hash de la contraseña
        var hashedPassword = _passwordHasher.HashPassword(request.Contrasena);
        var passwordHashResult = PasswordHash.Create(hashedPassword);

        var nombreResult = Nombre.Create(request.Nombre);
        var apellidosResult = Apellido.Create(request.Apellidos);

        // 3. Crear el usuario usando el método Factory del dominio
        var newUsuario = Usuario.Create(emailResult.Value, nombreResult.Value, apellidosResult.Value, passwordHashResult.Value);

        // 4. Guardar en el repositorio
        await _usuarioWriteRepository.CreateAsync(newUsuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Encolar email de confirmación (no bloqueante)
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:4200";
        var confirmUrl = $"{baseUrl}/auth/confirmar-correo?token={newUsuario.TokenConfirmacion!.Value.Value}";
        var nombrePila = newUsuario.Nombre!.Value; // Accedemos al string dentro del VO

        var emailBody = $@"
        <html>
            <body style='font-family: Arial, sans-serif; font-size: 16px; color: #333; line-height: 1.6;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                    
                    <h1 style='color: #2196F3; text-align: center;'>¡Hola, {nombrePila}!</h1>
                    
                    <p>Gracias por registrarte en <strong>AhorroLand</strong>. Estamos felices de tenerte aquí.</p>
                    
                    <p>Para comenzar a gestionar tus finanzas, por favor confirma que este es tu correo electrónico haciendo clic en el botón de abajo:</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmUrl}' target='_blank' 
                           style='background-color: #2196F3; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold; display: inline-block;'>
                           Confirmar mi cuenta
                        </a>
                    </div>
                    
                    <p style='font-size: 14px; color: #777;'>
                        Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:<br>
                        <a href='{confirmUrl}' style='color: #1a73e8;'>{confirmUrl}</a>
                    </p>
                </div>
            </body>
        </html>";

        var emailMessage = new EmailMessage(
            newUsuario.Correo.Value,
            "Bienvenido a AhorroLand - Confirma tu correo",
            emailBody
        );

        _emailService.EnqueueEmail(emailMessage);

        return Result.Success();
    }
}

