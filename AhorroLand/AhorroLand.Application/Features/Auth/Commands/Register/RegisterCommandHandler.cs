using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Abstractions.Services;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
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
        try
        {
            // 1. Validar que el correo no exista
            var emailVO = new Email(request.Correo);
            var existingUser = await _usuarioReadRepository.GetByEmailAsync(emailVO, cancellationToken);

            if (existingUser != null)
            {
                return Result.Failure(new Error("Auth.EmailExists", $"El correo '{request.Correo}' ya está registrado."));
            }

            // 2. Hash de la contraseña
            var hashedPassword = _passwordHasher.HashPassword(request.Contrasena);
            var passwordHashVO = new PasswordHash(hashedPassword);

            // 3. Crear el usuario usando el método Factory del dominio
            var newUsuario = Usuario.Create(emailVO, passwordHashVO);

            // 4. Guardar en el repositorio
            await _usuarioWriteRepository.CreateAsync(newUsuario, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Encolar email de confirmación (no bloqueante)
            var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:4200";
            var confirmUrl = $"{baseUrl}/auth/confirmar-correo?token={newUsuario.TokenConfirmacion!.Value.Value}";

            var emailBody = $@"
    <html>
              <body style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
       <h1>Gracias por registrarte en AhorroLand</h1>
        <p>Estamos felices de tenerte aquí. Por favor accede al siguiente enlace para verificar y activar tu cuenta:</p>
         <p><a href='{confirmUrl}' target='_blank' style='color: #1a73e8; text-decoration: none;'>Confirmar mi cuenta</a></p>
   </body>
       </html>";

            var emailMessage = new EmailMessage(
                newUsuario.Correo.Value,
                "Bienvenido a AhorroLand",
                emailBody
            );

            _emailService.EnqueueEmail(emailMessage);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Auth.RegisterError", ex.Message));
        }
    }
}

