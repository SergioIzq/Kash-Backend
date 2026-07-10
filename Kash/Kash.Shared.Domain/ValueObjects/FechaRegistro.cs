using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Shared.Domain.ValueObjects;

public readonly record struct FechaRegistro
{
    public DateTime Valor { get; }

    [Obsolete("No usar directamente. Utiliza FechaRegistro.Create() para validación o FechaRegistro.CreateFromDatabase() desde infraestructura.", error: true)]
    public FechaRegistro()
    {
        Valor = DateTime.MinValue;
    }

    private FechaRegistro(DateTime valor)
    {
        Valor = valor;
    }

    public static Result<FechaRegistro> Create(DateTime valor)
    {
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