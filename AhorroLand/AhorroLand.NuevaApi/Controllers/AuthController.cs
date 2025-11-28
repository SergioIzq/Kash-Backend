using AhorroLand.Application.Features.Auth.Commands.ConfirmEmail;
using AhorroLand.Application.Features.Auth.Commands.Login;
using AhorroLand.Application.Features.Auth.Commands.Register;
using AhorroLand.Application.Features.Auth.Commands.ResendConfirmationEmail;
using AhorroLand.NuevaApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AhorroLand.NuevaApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IMediator mediator, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="command">Datos del usuario a registrar.</param>
    /// <returns>Mensaje de confirmación.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { mensaje = result.Error.Name });
        }

        return Ok(result);
    }

    /// <summary>
    /// Inicia sesión y devuelve un token JWT (también puede establecerse en cookie).
    /// </summary>
    /// <param name="command">Credenciales de inicio de sesión.</param>
    /// <param name="useCookie">Si true, establece el token en una cookie HttpOnly en lugar de devolverlo en el body.</param>
    /// <returns>Token JWT y fecha de expiración.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
 [FromBody] LoginCommand command,
        [FromQuery] bool useCookie = false)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return Unauthorized(new { mensaje = result.Error.Message });
        }

        // Si useCookie es true, establecer token en cookie HttpOnly
        if (useCookie)
        {
            var loginResponse = result.Value;

            // Establecer cookie de autenticación
            Response.SetAuthCookie(
           loginResponse.Token,
               loginResponse.ExpiresAt,
         _environment.IsDevelopment()
          );

            // Retornar respuesta sin el token (ya está en la cookie)
            return Ok(new
            {
                mensaje = "Inicio de sesión exitoso",
                expiresAt = loginResponse.ExpiresAt,
                usandoCookie = true
            });
        }

        // Modo clásico: retornar token en el body
        return Ok(result.Value);
    }

    /// <summary>
    /// Cierra la sesión del usuario eliminando las cookies de autenticación.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // Eliminar cookies de autenticación
        Response.ClearAuthCookies();

        return Ok(new { mensaje = "Sesión cerrada exitosamente" });
    }

    /// <summary>
    /// Confirma el correo electrónico del usuario.
    /// </summary>
    /// <param name="token">Token de confirmación.</param>
    /// <returns>Mensaje de confirmación.</returns>
    [HttpGet("confirmar-correo")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarCorreo([FromQuery] string token)
    {
        var command = new ConfirmEmailCommand(token);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { mensaje = result.Error.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationEmailCommand request)
    {
        var command = new ResendConfirmationEmailCommand(request.Correo);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
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
}
