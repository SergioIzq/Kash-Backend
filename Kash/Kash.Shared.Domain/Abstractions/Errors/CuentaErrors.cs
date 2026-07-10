using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Domain.Errors;

public static class CuentaErrors
{
    public static Error NombreDuplicado(string nombre) => Error.Conflict(
        $"Ya existe una cuenta con el nombre '{nombre}'."
    );
}
