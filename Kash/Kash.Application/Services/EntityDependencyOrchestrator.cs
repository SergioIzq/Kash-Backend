using Kash.Shared.Application.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Application.Services;

/// <summary>
/// Implementación de <see cref="IEntityDependencyOrchestrator"/>: recorre los pasos en orden,
/// encadenando los Guids ya resueltos como additionalData de los pasos siguientes.
/// </summary>
public sealed class EntityDependencyOrchestrator : IEntityDependencyOrchestrator
{
    public async Task<Result<Dictionary<string, object>>> ResolveAsync(
        Guid usuarioId,
        IReadOnlyList<DependencyStep> steps,
        CancellationToken cancellationToken = default)
    {
        var dependencies = new Dictionary<string, object>();
        var resolvedGuids = new Dictionary<string, Guid>();

        foreach (var step in steps)
        {
            var additionalData = step.AdditionalData?.Invoke(resolvedGuids);

            var resolvedGuid = await step.FindOrCreateAsync(
                step.Id,
                step.Nombre,
                usuarioId,
                additionalData,
                cancellationToken);

            if (resolvedGuid is null)
            {
                if (step.Required)
                {
                    return Result.Failure<Dictionary<string, object>>(Error.Validation(
                        step.RequiredErrorMessage ?? $"No se pudo resolver la dependencia '{step.Key}'."));
                }

                continue;
            }

            resolvedGuids[step.Key] = resolvedGuid.Value;
            dependencies[step.Key] = step.ToDependencyValue(resolvedGuid.Value);
        }

        return Result.Success(dependencies);
    }
}
