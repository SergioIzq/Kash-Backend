using AhorroLand.Shared.Domain.Abstractions.Results;

namespace AhorroLand.Shared.Domain.ValueObjects;

public readonly record struct Cantidad
{
    public decimal Valor { get; }

    [Obsolete("No usar directamente. Utiliza Cantidad.Create() para validación o Cantidad.CreateFromDatabase() desde infraestructura.", error: true)]
    public Cantidad()
    {
        Valor = 0;
    }

    private Cantidad(decimal valor)
    {
        Valor = valor;
    }

    public static Result<Cantidad> Create(decimal valor)
    {
        // REGLA DE DOMINIO: El saldo no puede ser negativo
        if (valor < 0)
        {
            return Result.Failure<Cantidad>(Error.Validation("La cantidad no puede ser negativa."));
        }

        if (valor != Math.Round(valor, 2))
        {
            return Result.Failure<Cantidad>(Error.Validation($"La cantidad tiene más de dos decimales significativos. Valor recibido: {valor}"));
        }

        return Result.Success(new Cantidad(valor));
    }

    public static Cantidad CreateFromDatabase(decimal valor) => new Cantidad(valor);

    public Cantidad Sumar(Cantidad otro) => new Cantidad(this.Valor + otro.Valor);
    public Cantidad Restar(Cantidad otro) => new Cantidad(this.Valor - otro.Valor);
    public static Cantidad Zero() => new Cantidad(0);
}