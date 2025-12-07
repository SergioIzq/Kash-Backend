using AhorroLand.Shared.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AhorroLand.Infrastructure.Services.Auth;


public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
            {
                return null;
            }

            // Busca el Claim que contiene el ID. 
            // Normalmente es ClaimTypes.NameIdentifier o "sub" dependiendo de tu configuración de JWT.
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("sub")?.Value
                          ?? user.FindFirst("uid")?.Value;

            if (Guid.TryParse(idClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
