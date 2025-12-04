using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct FechaEjecucion
{
    public DateTime Valor { get; }

    [Obsolete("No usar directamente. Utiliza FechaEjecucion.Create() para validación o FechaEjecucion.CreateFromDatabase() desde infraestructura.", error: true)]
    public FechaEjecucion()
    {
        Valor = DateTime.MinValue;
    }

    private FechaEjecucion(DateTime valor)
    {
        Valor = valor;
    }

    public static Result<FechaEjecucion> Create(DateTime valor)
    {
        if (valor == DateTime.MinValue)
        {
            return Result.Failure<FechaEjecucion>(Error.Validation("La fecha proporcionada no es válida."));
        }

        return Result.Success(new FechaEjecucion(valor));
    }

    public static FechaEjecucion CreateFromDatabase(DateTime valor) => new FechaEjecucion(valor);
}