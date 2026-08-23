using Kash.Application.Features.Auth.Commands.ConfirmEmail;
using Kash.Application.Features.Auth.Commands.ForgotPassword;
using Kash.Application.Features.Auth.Commands.GenerateApiToken;
using Kash.Application.Features.Auth.Commands.Login;
using Kash.Application.Features.Auth.Commands.Register;
using Kash.Application.Features.Auth.Commands.ResendConfirmationEmail;
using Kash.Application.Features.Auth.Commands.ResetPassword;
using Kash.Application.Features.Auth.Commands.UpdateUserProfile;
using Kash.Application.Features.Auth.Commands.UploadAvatar;
using Kash.Application.Features.Auth.Queries;
using SergioIzq.AspNetCore.Kernel.Controllers; // Usamos tu controlador base
using Kash.Api.Extensions; // Para cookies si las usas como extensiones
using SergioIzq.Domain.Kernel.Abstractions.Results; // Para Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kash.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : AbsController // Heredamos de AbsController
{
    private readonly IWebHostEnvironment _environment;

    // Pasamos el Sender al padre
    public AuthController(ISender sender, IWebHostEnvironment environment)
        : base(sender)
    {
        _environment = environment;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _sender.Send(command);

        // Usamos HandleResult para devolver la estructura estandarizada
        return HandleResult(result);
    }

    /// <summary>
    /// Inicia sesión y devuelve un token JWT.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        [FromQuery] bool useCookie = false)
    {
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            // El padre se encarga de mapear el error (401, 400, etc.)
            return HandleResult(result);
        }

        // Lógica específica de Cookies (Solo si es éxito)
        if (useCookie)
        {
            var loginResponse = result.Value;

            // Opción A: Usar método helper del AbsController (si lo agregaste)
            // SetCookie("authToken", loginResponse.Token, ...);

            // Opción B: Usar tu extensión actual
            Response.SetAuthCookie(
                loginResponse.Token,
                loginResponse.ExpiresAt,
                _environment.IsDevelopment()
            );

            // Devolvemos éxito pero sin el token en el body (por seguridad de cookie)
            // Creamos un Result modificado solo para la vista
            return Ok(Result.Success(new
            {
                expiresAt = loginResponse.ExpiresAt,
                usandoCookie = true
            }));
        }

        // Modo clásico: Token en el body
        return HandleResult(result);
    }

    /// <summary>
    /// Cierra la sesión del usuario.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // 1. Limpiar cookies del lado del cliente
        // Opción A: DeleteCookie("authToken");
        // Opción B: Extensión
        Response.ClearAuthCookies();

        // 2. Llamar al backend por si hay lógica de invalidación de Refresh Tokens (blacklist)
        // Aunque el comando sea void, es buena práctica pasar por el handler.
        // Si no tienes comando de Logout, solo retorna Ok.
        // var result = await _sender.Send(new LogoutCommand()); 

        return HandleResult(Result.Success("Sesión cerrada correctamente"));
    }

    /// <summary>
    /// Confirma el correo electrónico del usuario.
    /// </summary>
    [HttpGet("confirmar-correo")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarCorreo([FromQuery] string token)
    {
        var command = new ConfirmEmailCommand(token);
        return await SendAndHandleAsync(command);
    }

    [HttpPost("resend-confirmation")]
    [AllowAnonymous] // Usualmente se permite reenviar sin estar logueado si olvidaste confirmar
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationEmailCommand request)
    {
        return await SendAndHandleAsync(request);
    }

    /// <summary>
    /// Obtiene información completa del usuario desde la base de datos.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        // 1. Obtenemos el ID del claim (Token)
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado.")));
        }

        // 2. Lanzamos la Query para buscar los datos reales en BD
        var query = new GetUserProfileQuery(userId.Value);
        var result = await _sender.Send(query);

        // 3. Devolvemos el resultado (que ahora incluye nombre, apellido, etc.)
        return HandleResult(result);
    }

    /// <summary>
    /// Genera (o regenera) el token de API personal del usuario, pensado para integraciones
    /// de larga duración (p. ej. un Atajo de iOS) que no pueden depender del JWT de sesión de
    /// 12h. El valor en claro solo se devuelve en esta respuesta; regenerarlo invalida el
    /// token anterior de inmediato.
    /// </summary>
    [HttpPost("api-token")]
    [Authorize]
    public async Task<IActionResult> GenerateApiToken()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado.")));
        }

        var command = new GenerateApiTokenCommand(userId.Value);
        return await SendAndHandleAsync(command);
    }

    /// <summary>
    /// Indica si el usuario tiene un token de API activo y desde cuándo, sin revelar su valor.
    /// </summary>
    [HttpGet("api-token")]
    [Authorize]
    public async Task<IActionResult> GetApiTokenStatus()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado.")));
        }

        var query = new GetApiTokenStatusQuery(userId.Value);
        return await SendAndHandleAsync(query);
    }

    /// <summary>
    /// Solicita recuperación de contraseña.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _sender.Send(command);

        // Nota: HandleResult devolverá el error si el correo no existe Y tu Handler devuelve Failure.
        // Si por seguridad tu Handler devuelve Success aunque el correo no exista (para no enumerar),
        // HandleResult devolverá 200 OK, lo cual es correcto.
        return HandleResult(result);
    }

    /// <summary>
    /// Restablece la contraseña.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        return await SendAndHandleAsync(command);
    }

    /// <summary>
    /// Actualiza los datos del perfil del usuario (Nombre, Apellido).
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado.")));
        }

        var command = new UpdateUserProfileCommand(
            userId.Value,
            request.Nombre,
            request.Apellidos
        );

        return await SendAndHandleAsync(command);
    }

    /// <summary>
    /// Sube o actualiza la foto de perfil del usuario.
    /// Acepta archivos de imagen (jpg, png, webp) hasta 5MB.
    /// </summary>
    /// <param name="file">El archivo de imagen enviado como multipart/form-data.</param>
    [HttpPost("avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequest request)
    {
        // 1. Obtener usuario autenticado
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized(Result.Failure(Error.Unauthorized("Usuario no autenticado.")));

        var file = request.File; // Sacamos el archivo del DTO

        // 2. Validaciones rápidas de entrada (Fail Fast)
        if (file == null || file.Length == 0)
        {
            return BadRequest(Result.Failure(Error.Validation("No se ha proporcionado ningún archivo.")));
        }

        // Validación de tipo MIME (Solo imágenes)
        if (!file.ContentType.StartsWith("image/"))
        {
            return BadRequest(Result.Failure(Error.Validation("El archivo debe ser una imagen válida (JPG, PNG, WEBP).")));
        }

        // Validación de tamaño (Ejemplo: Máximo 5MB)
        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            return BadRequest(Result.Failure(Error.Validation("La imagen no puede exceder los 5MB.")));
        }

        // 3. Preparar el comando
        // Usamos 'using' para asegurar que el stream se cierre correctamente al terminar
        using var stream = file.OpenReadStream();

        var command = new UploadAvatarCommand(
            userId.Value,
            stream,
            file.FileName,
            file.ContentType
        );

        // 4. Enviar al Handler (que guardará en disco y actualizará la BD)
        var result = await _sender.Send(command);

        // 5. Retornar resultado
        // Si es exitoso, devolverá la URL del avatar en 'result.Value'
        return HandleResult(result);
    }

    public record UpdateProfileRequest(
        string Nombre,
        string? Apellidos
    );

    public record UploadAvatarRequest
    {
        // El nombre "file" aquí es lo que buscará el frontend en el FormData
        public required IFormFile File { get; set; }
    }
}
