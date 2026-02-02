using Kash.Shared.Domain.Abstractions.Results;

namespace Kash.Domain.Errors;

public static class PersonaErrors
{
    // Usamos Error.Conflict porque intenta crear algo que ya existe
    public static Error NombreDuplicado(string nombre) => Error.Conflict(
        $"Ya existe una persona con el nombre '{nombre}'."
    );
}