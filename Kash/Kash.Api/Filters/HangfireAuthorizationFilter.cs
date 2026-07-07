using Hangfire.Dashboard;

namespace Kash.Api;

/// <summary>
/// Filtro de autorizaci�n para Hangfire Dashboard.
/// En desarrollo permite acceso sin autenticaci�n.
/// En producci�n requiere autenticaci�n.
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

        // En producci�n, siempre requerir autenticaci�n
        // Nota: En Hangfire con ASP.NET Core, el contexto no tiene acceso directo a HttpContext
        // por lo que en producci�n se debe configurar autenticaci�n a nivel de endpoint
        return false;
    }
}
