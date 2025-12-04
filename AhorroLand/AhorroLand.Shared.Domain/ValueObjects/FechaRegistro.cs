using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct FechaRegistro
{
    public DateTime Valor { get; }

    private FechaRegistro(DateTime valor)
    {
        Valor = valor;
    }

    public static Result<FechaRegistro> Create(DateTime valor)
    {
        if (valor > DateTime.UtcNow)
        {
            return Result.Failure<FechaRegistro>(Error.Validation("La fecha de registro no puede ser una fecha futura."));
        }

        if (valor == DateTime.MinValue)
        {
            return Result.Failure<FechaRegistro>(Error.Validation("La fecha proporcionada no es válida."));
        }

        return Result.Success(new FechaRegistro(valor));
    }

    public static FechaRegistro CreateFromDatabase(DateTime valor) => new FechaRegistro(valor);

    public static FechaRegistro Hoy()
    {
        return new FechaRegistro(DateTime.Today);
    }
}