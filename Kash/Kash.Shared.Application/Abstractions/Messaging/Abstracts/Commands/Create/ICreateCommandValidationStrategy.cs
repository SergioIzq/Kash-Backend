using Kash.Shared.Domain.Abstractions.Results;

namespace Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;

/// <summary>
/// Estrategia de validación para comandos de creación.
/// Permite diferentes tipos de validación sin modificar el handler base.
/// </summary>
public interface ICreateCommandValidationStrategy<TCommand>
{
    /// <summary>
    /// Valida el comando antes de crear la entidad.
    /// </summary>
    /// <returns>Result.Success() si la validación es exitosa, Result.Failure() si falla</returns>
    Task<Result> ValidateAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Estrategia que no realiza ninguna validación adicional.
/// Para entidades simples que solo necesitan crear la entidad.
/// </summary>
public class NoValidationStrategy<TCommand> : ICreateCommandValidationStrategy<TCommand>
{
    public Task<Result> ValidateAsync(TCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }
}
