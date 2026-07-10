using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Domain.Errors;

public static class ClienteErrors
{
    // Usamos Error.Conflict porque intenta crear algo que ya existe
    public static Error NombreDuplicado(string nombre) => Error.Conflict(
        $"Ya existe un cliente con el nombre '{nombre}'."
    );
}