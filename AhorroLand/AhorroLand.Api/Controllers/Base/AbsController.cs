using AhorroLand.Shared.Domain.Abstractions.Enums;
using AhorroLand.Shared.Domain.Abstractions.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AhorroLand.NuevaApi.Controllers.Base;

[Authorize]
[ApiController]
public abstract class AbsController : ControllerBase
{
    protected readonly ISender _sender;

    protected AbsController(ISender sender)
    {
        _sender = sender;
    }

    // ==========================================
    // 1. MANEJO DE RESULTADOS GENÉRICOS (GET)
    // ==========================================

    /// <summary>
    /// Retorna 200 OK con el valor si es exitoso, o el error correspondiente.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        // Devolvemos el Result completo. 
        // Angular recibirá: { "isSuccess": true, "value": { ...data... }, "error": ... }
        return Ok(result);
    }

    // ==========================================
    // 2. MANEJO DE ACCIONES VACÍAS (UPDATE/DELETE)
    // ==========================================

    /// <summary>
    /// Retorna 204 No Content si es exitoso (ideal para Update/Delete), o el error.
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }

    // ==========================================
    // 3. MANEJO DE CREACIÓN (POST)
    // ==========================================

    /// <summary>
    /// Retorna 201 Created con Location Header si se provee ruta, o 200 OK.
    /// </summary>
    protected IActionResult HandleResultForCreation<T>(Result<T> result, string? actionName = null, object? routeValues = null)
    {
        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        // Si especificamos una acción (ej. "GetUserById"), devolvemos 201 con cabecera Location
        if (!string.IsNullOrEmpty(actionName) && routeValues != null)
        {
            return CreatedAtAction(actionName, routeValues, result);
        }

        // Si no, simplemente un 200/201 genérico con el body
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ==========================================
    // 4. MANEJO DE ERRORES CENTRALIZADO
    // ==========================================

    /// <summary>
    /// Convierte un Error de Dominio en una respuesta HTTP con el código correcto.
    /// </summary>
    private IActionResult HandleFailure(Error error)
    {
        // Mapeo limpio basado en el Enum (No strings mágicos)
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError // Failure por defecto
        };

        var failureResult = Result.Failure(error);
        return StatusCode(statusCode, failureResult);
    }

    // ==========================================
    // 5. HELPERS DE INFRAESTRUCTURA (Cookies/User)
    // ==========================================

    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    protected void SetRefreshTokenCookie(string token, int expireDays = 7)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(expireDays),
            SameSite = SameSiteMode.Strict,
            Secure = !IsDevelopment()
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private bool IsDevelopment()
    {
        return HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;
    }
}