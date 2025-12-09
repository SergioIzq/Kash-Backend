namespace AhorroLand.NuevaApi.Extensions;

/// <summary>
/// Extensiones para el manejo seguro de cookies en la API.
/// </summary>
public static class CookieExtensions
{
    /// <summary>
    /// Establece una cookie de autenticación (AccessToken) con configuración segura.
    /// </summary>
    public static void SetAuthCookie(this HttpResponse response, string token, DateTime expiration, bool isDevelopment)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // No accesible desde JavaScript (XSS protection)
            Secure = !isDevelopment, // Solo HTTPS en producción
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = expiration,
            Path = "/",
            IsEssential = true
        };

        response.Cookies.Append("AccessToken", token, cookieOptions);
    }

    /// <summary>
    /// Establece una cookie de refresh token con configuración ultra segura.
    /// </summary>
    public static void SetRefreshTokenCookie(this HttpResponse response, string refreshToken, DateTime expiration, bool isDevelopment)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.Strict,
            Expires = expiration,
            Path = "/api/auth/refresh", // Solo accesible en el endpoint de refresh
            IsEssential = true
        };

        response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);
    }

    /// <summary>
    /// Elimina las cookies de autenticación (logout).
    /// </summary>
    public static void ClearAuthCookies(this HttpResponse response)
    {
        response.Cookies.Delete("AccessToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        response.Cookies.Delete("RefreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth/refresh"
        });
    }

    /// <summary>
    /// Obtiene el token de una cookie de forma segura.
    /// </summary>
    public static string? GetAuthToken(this HttpRequest request)
    {
        return request.Cookies["AccessToken"];
    }

    /// <summary>
    /// Obtiene el refresh token de una cookie de forma segura.
    /// </summary>
    public static string? GetRefreshToken(this HttpRequest request)
    {
        return request.Cookies["RefreshToken"];
    }

    /// <summary>
    /// Establece una cookie genérica con configuración personalizable.
    /// </summary>
    public static void SetCookie(
        this HttpResponse response,
 string key,
        string value,
        int? expireMinutes = null,
    bool httpOnly = true,
        bool secure = true,
     SameSiteMode sameSite = SameSiteMode.Strict)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = httpOnly,
            Secure = secure,
            SameSite = sameSite,
            Path = "/",
            IsEssential = true
        };

        if (expireMinutes.HasValue)
        {
            cookieOptions.Expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes.Value);
        }

        response.Cookies.Append(key, value, cookieOptions);
    }

    /// <summary>
    /// Obtiene el valor de una cookie de forma segura.
    /// </summary>
    public static string? GetCookie(this HttpRequest request, string key)
    {
        return request.Cookies[key];
    }

    /// <summary>
    /// Elimina una cookie específica.
    /// </summary>
    public static void DeleteCookie(this HttpResponse response, string key)
    {
        response.Cookies.Delete(key, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }
}
