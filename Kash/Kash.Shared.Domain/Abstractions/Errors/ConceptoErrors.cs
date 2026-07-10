using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Domain.Errors;

public static class ConceptoErrors
{
    public static Error NombreDuplicado(string nombre) => Error.Conflict(
        $"Ya existe un concepto con el nombre '{nombre}'."
    );
}
