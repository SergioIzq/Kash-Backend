using SergioIzq.Domain.Kernel.Abstractions.Errors;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Domain.Errors;

public static class PersonaErrors
{
    public static Error NombreDuplicado(string nombre) => EntityErrors.DuplicateName("una persona", nombre);
}
