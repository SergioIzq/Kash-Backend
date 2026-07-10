using SergioIzq.Domain.Kernel.Abstractions.Errors;
using SergioIzq.Domain.Kernel.Abstractions.Results;

namespace Kash.Domain.Errors;

public static class FormaPagoErrors
{
    public static Error NombreDuplicado(string nombre) => EntityErrors.DuplicateName("una forma de pago", nombre);
}
