using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Shared.Application.Interfaces;

/// <summary>
/// Un paso de resolución de dependencia: busca-o-crea una entidad relacionada
/// (Categoría, Concepto, Cuenta...) a partir de un Id/Nombre y la registra bajo <see cref="Key"/>.
/// </summary>
/// <param name="Key">Clave bajo la que se guarda el resultado (p.ej. "CategoriaId").</param>
/// <param name="Id">Id de la entidad si el llamante ya lo conoce.</param>
/// <param name="Nombre">Nombre para buscar/crear la entidad si no se conoce el Id.</param>
/// <param name="FindOrCreateAsync">El método FindOrCreateAsync del finder-service concreto.</param>
/// <param name="ToDependencyValue">Convierte el Guid resuelto al Value Object tipado (p.ej. CategoriaId).</param>
/// <param name="Required">Si es true, la ausencia de resultado hace fallar la orquestación completa.</param>
/// <param name="RequiredErrorMessage">Mensaje de error de validación cuando un paso requerido no se resuelve.</param>
/// <param name="AdditionalData">
/// Construye el additionalData para este paso a partir de los Guids ya resueltos en pasos anteriores
/// (p.ej. Concepto necesita la CategoriaId ya resuelta por el paso previo).
/// </param>
public sealed record DependencyStep(
    string Key,
    Guid? Id,
    string? Nombre,
    Func<Guid?, string?, Guid, Dictionary<string, object>?, CancellationToken, Task<Guid?>> FindOrCreateAsync,
    Func<Guid, object> ToDependencyValue,
    bool Required = true,
    string? RequiredErrorMessage = null,
    Func<IReadOnlyDictionary<string, Guid>, Dictionary<string, object>>? AdditionalData = null);

/// <summary>
/// Orquesta una secuencia ordenada de <see cref="DependencyStep"/> para los handlers de
/// Create/Update que necesitan buscar-o-crear varias entidades relacionadas antes de
/// construir la entidad principal (p.ej. Gasto necesita Categoria, Concepto, Cuenta...).
/// </summary>
public interface IEntityDependencyOrchestrator : IApplicationService
{
    Task<Result<Dictionary<string, object>>> ResolveAsync(
        Guid usuarioId,
        IReadOnlyList<DependencyStep> steps,
        CancellationToken cancellationToken = default);
}
