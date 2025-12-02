using AhorroLand.Application.Features.Auth.Commands.ConfirmEmail;
using AhorroLand.Application.Features.Auth.Commands.ForgotPassword;
using AhorroLand.Application.Features.Auth.Commands.Login;
using AhorroLand.Application.Features.Auth.Commands.Register;
using AhorroLand.Application.Features.Auth.Commands.ResendConfirmationEmail;
using AhorroLand.Application.Features.Auth.Commands.ResetPassword;
using AhorroLand.NuevaApi.Controllers.Base; // ✅ Usamos tu controlador base
using AhorroLand.NuevaApi.Extensions; // Para cookies si las usas como extensiones
using AhorroLand.Shared.Domain.Abstractions.Results; // Para Result
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AhorroLand.NuevaApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : AbsController // ✅ Heredamos de AbsController
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
    [Authorize] // O AllowAnonymous si quieres permitir logout sin token válido (limpieza)
    public async Task<IActionResult> Logout()
    {
        // 1. Limpiar cookies del lado del cliente
        // Opción A: DeleteCookie("authToken");
        // Opción B: Extensión
        Response.ClearAuthCookies();

        // 2. Llamar al backend por si hay lógica de invalidación de Refresh Tokens (blacklist)
        // Aunque el comando sea void, es buena práctica pasar por el handler.
        // Si no tienes comando de Logout, solo retorna Ok.
        // var result = await _sender.Send(new LogoutCommand()); 

        return Ok(Result.Success("Sesión cerrada correctamente"));
    }

    /// <summary>
    /// Confirma el correo electrónico del usuario.
    /// </summary>
    [HttpGet("confirmar-correo")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarCorreo([FromQuery] string token)
    {
        var command = new ConfirmEmailCommand(token);
        var result = await _sender.Send(command);

        return HandleResult(result);
    }

    [HttpPost("resend-confirmation")]
    [AllowAnonymous] // Usualmente se permite reenviar sin estar logueado si olvidaste confirmar
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationEmailCommand request)
    {
        var result = await _sender.Send(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Obtiene información del usuario actualmente autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new
        {
            userId = Guid.Parse(userId ?? Guid.Empty.ToString()),
            email
        });
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
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}