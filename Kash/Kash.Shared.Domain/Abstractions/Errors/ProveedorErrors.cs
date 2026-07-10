using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Domain.Errors;

public static class ProveedorErrors
{
    public static Error NombreDuplicado(string nombre) => Error.Conflict(
        $"Ya existe un proveedor con el nombre '{nombre}'."
    );
}
