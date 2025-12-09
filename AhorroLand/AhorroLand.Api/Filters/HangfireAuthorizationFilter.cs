using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace AhorroLand.NuevaApi;

/// <summary>
/// Filtro de autorización para Hangfire Dashboard.
/// En desarrollo permite acceso sin autenticación.
/// En producción requiere autenticación.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
  // Si estamos en desarrollo, permitir acceso
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
 return true;
        }

        // En producción, siempre requerir autenticación
  // Nota: En Hangfire con ASP.NET Core, el contexto no tiene acceso directo a HttpContext
 // por lo que en producción se debe configurar autenticación a nivel de endpoint
return false;
    }
}
