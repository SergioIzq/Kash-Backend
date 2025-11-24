using AhorroLand.NuevaApi.Extensions;
using AhorroLand.NuevaApi.Models.Responses;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Results;
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

    /// <summary>
    /// Maneja el resultado y lo envuelve en ApiResponse
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.Ok(result.Value);
            return Ok(response);
        }

        return HandleFailure(result.Error);
    }

    /// <summary>
    /// Maneja resultados sin valor (Update, Delete)
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent(); // ✅ 204 No Content para DELETE/UPDATE exitosos
        }

        return HandleFailure(result.Error);
    }

    /// <summary>
    /// Maneja resultado de creación (devuelve 201 Created con ApiResponse)
    /// </summary>
    protected IActionResult HandleResultForCreation<T>(Result<T> result, string actionName, object routeValues)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.Ok(result.Value, "Recurso creado exitosamente");
            return CreatedAtAction(actionName, routeValues, response);
        }

        return HandleFailure(result.Error);
    }

    /// <summary>
    /// 🆕 Maneja resultado paginado y lo envuelve en ApiResponse con PaginatedResponse
    /// </summary>
    protected IActionResult HandlePagedResult<T>(Result<PagedList<T>> result)
    {
        if (result.IsSuccess)
        {
            var pagedData = result.Value;
            var paginatedResponse = new PaginatedResponse<T>(
                pagedData.Items,
                pagedData.TotalCount,
                pagedData.Page,
                pagedData.PageSize
            );

            var response = ApiResponse<PaginatedResponse<T>>.Ok(paginatedResponse);
            return Ok(response);
        }

        return HandleFailure(result.Error);
    }

    /// <summary>
    /// 🆕 Maneja lista simple (search, recent) y la envuelve en ApiResponse con ListResponse
    /// </summary>
    protected IActionResult HandleListResult<T>(Result<IEnumerable<T>> result, string? message = null)
    {
        if (result.IsSuccess)
        {
            var listResponse = new ListResponse<T>(result.Value);
            var response = ApiResponse<ListResponse<T>>.Ok(listResponse, message);
            return Ok(response);
        }

        return HandleFailure(result.Error);
    }

    /// <summary>
    /// ✅ Maneja errores y devuelve el código HTTP apropiado según el código de error
    /// </summary>
    private IActionResult HandleFailure(Error error)
    {
        // Determinar el código de estado HTTP según el código de error
        var statusCode = error.Code switch
        {
            "Error.NotFound" => StatusCodes.Status404NotFound,
            "Error.Conflict" => StatusCodes.Status409Conflict,
            "Error.Validation" => StatusCodes.Status400BadRequest,
            "Error.UpdateFailure" => StatusCodes.Status500InternalServerError,
            "Error.DeleteFailure" => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        // Crear respuesta de error usando el formato de ApiResponse
        var errorResponse = new
        {
            success = false,
            error = new
            {
                code = error.Code,
                title = error.Name,
                detail = error.Message
            },
            timestamp = DateTime.UtcNow
        };

        return StatusCode(statusCode, errorResponse);
    }

    // 🍪 Métodos helper para cookies

    /// <summary>
    /// Establece una cookie de forma segura.
    /// </summary>
    protected void SetCookie(string key, string value, int? expireMinutes = null, bool httpOnly = true)
    {
        Response.SetCookie(key, value, expireMinutes, httpOnly, secure: !IsDevelopment(), SameSiteMode.Strict);
    }

    /// <summary>
    /// Obtiene el valor de una cookie.
    /// </summary>
    protected string? GetCookie(string key)
    {
        return Request.GetCookie(key);
    }

    /// <summary>
    /// Elimina una cookie.
    /// </summary>
    protected void DeleteCookie(string key)
    {
        Response.DeleteCookie(key);
    }

    /// <summary>
    /// Obtiene el ID del usuario autenticado desde los claims.
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Obtiene el email del usuario autenticado desde los claims.
    /// </summary>
    protected string? GetCurrentUserEmail()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Verifica si la aplicación está en modo desarrollo.
    /// </summary>
    private bool IsDevelopment()
    {
        return HttpContext.RequestServices
            .GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;
    }
}
