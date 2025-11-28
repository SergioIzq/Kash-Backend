using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AhorroLand.Middleware.Filters;

public class ResultMappingFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Interceptamos si el resultado es un ObjectResult (lo que devuelven los controladores)
        if (context.Result is ObjectResult objectResult &&
            objectResult.Value is not null)
        {
            var valueType = objectResult.Value.GetType();

            // Usamos reflexión o dynamic para verificar si es un Result<T> o Result genérico
            // Asumiendo que tu clase Result tiene propiedades 'IsSuccess' y 'Error'
            // Nota: Para máximo rendimiento, evita dynamic y usa interfaces (ej: IResultBase)

            dynamic result = objectResult.Value;

            // Verificamos si parece ser tu objeto Result
            var isResult = valueType.GetProperty("IsSuccess") != null &&
                           valueType.GetProperty("Error") != null;

            if (isResult && !result.IsSuccess) // Si falló
            {
                var error = result.Error;
                var statusCode = MapErrorCodeToHttpStatus(error.Code);

                // Creamos la nueva respuesta de error
                var errorResponse = new ErrorResponse(
                    error.Code,
                    error.Name ?? "Error",
                    error.Message,
                    System.Diagnostics.Activity.Current?.Id
                );

                // REEMPLAZAMOS el resultado. 
                // ASP.NET Core serializará esto automáticamente una sola vez.
                context.Result = new ObjectResult(errorResponse)
                {
                    StatusCode = statusCode
                };
            }
        }
    }

    private static int MapErrorCodeToHttpStatus(string errorCode)
    {
        // Tu lógica de mapeo optimizada con Dictionary o Switch Expression
        if (string.IsNullOrEmpty(errorCode)) return 500;

        return errorCode switch
        {
            var c when c.Contains("Validation", StringComparison.OrdinalIgnoreCase) => 400,
            var c when c.Contains("NotFound", StringComparison.OrdinalIgnoreCase) => 404,
            var c when c.Contains("Conflict", StringComparison.OrdinalIgnoreCase) => 409,
            var c when c.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase) => 409,
            var c when c.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) => 401,
            var c when c.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) => 403,
            _ => 500
        };
    }
}
