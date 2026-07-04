using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kash.Domain;

/// <summary>
/// Regla de auto-categorización: si la descripción de un movimiento importado contiene
/// alguno de los patrones (separados por coma), se propone la clasificación indicada
/// en lugar de los valores por defecto del mapeo de importación.
/// </summary>
[Table("reglas_categorizacion")]
public sealed class ReglaCategorizacion : AbsEntity<ReglaCategorizacionId>
{
    public const int PatronMaxLength = 200;
    public const int NombreMaxLength = 100;

    // Constructor privado sin parámetros para EF Core
    private ReglaCategorizacion() : base(ReglaCategorizacionId.Create(Guid.NewGuid()).Value)
    {
    }

    private ReglaCategorizacion(
        ReglaCategorizacionId id,
        string patron,
        string? tipo,
        string categoriaNombre,
        string? conceptoNombre,
        string? proveedorNombre,
        string? formaPagoNombre,
        int prioridad,
        bool activo,
        UsuarioId usuarioId) : base(id)
    {
        Patron = patron;
        Tipo = tipo;
        CategoriaNombre = categoriaNombre;
        ConceptoNombre = conceptoNombre;
        ProveedorNombre = proveedorNombre;
        FormaPagoNombre = formaPagoNombre;
        Prioridad = prioridad;
        Activo = activo;
        UsuarioId = usuarioId;
    }

    /// <summary>Palabra(s) clave a buscar en la descripción del movimiento, separadas por coma.</summary>
    public string Patron { get; private set; } = string.Empty;

    /// <summary>"gasto" | "ingreso" | null (se aplica a cualquiera de los dos).</summary>
    public string? Tipo { get; private set; }

    public string CategoriaNombre { get; private set; } = string.Empty;
    public string? ConceptoNombre { get; private set; }
    public string? ProveedorNombre { get; private set; }
    public string? FormaPagoNombre { get; private set; }

    /// <summary>Orden de evaluación: las reglas con número más bajo se comprueban primero.</summary>
    public int Prioridad { get; private set; }

    public bool Activo { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public static Result<ReglaCategorizacion> Create(
        string patron,
        string? tipo,
        string categoriaNombre,
        string? conceptoNombre,
        string? proveedorNombre,
        string? formaPagoNombre,
        int prioridad,
        bool activo,
        UsuarioId usuarioId)
    {
        var validation = Validar(patron, tipo, categoriaNombre, conceptoNombre, proveedorNombre, formaPagoNombre);
        if (validation.IsFailure)
        {
            return Result.Failure<ReglaCategorizacion>(validation.Error);
        }

        var regla = new ReglaCategorizacion(
            ReglaCategorizacionId.Create(Guid.NewGuid()).Value,
            patron.Trim(),
            NormalizarTipo(tipo),
            categoriaNombre.Trim(),
            NullIfBlank(conceptoNombre),
            NullIfBlank(proveedorNombre),
            NullIfBlank(formaPagoNombre),
            prioridad,
            activo,
            usuarioId);

        return Result.Success(regla);
    }

    /// <summary>Actualiza todos los datos editables de la regla.</summary>
    public Result Update(
        string patron,
        string? tipo,
        string categoriaNombre,
        string? conceptoNombre,
        string? proveedorNombre,
        string? formaPagoNombre,
        int prioridad,
        bool activo)
    {
        var validation = Validar(patron, tipo, categoriaNombre, conceptoNombre, proveedorNombre, formaPagoNombre);
        if (validation.IsFailure)
        {
            return validation;
        }

        Patron = patron.Trim();
        Tipo = NormalizarTipo(tipo);
        CategoriaNombre = categoriaNombre.Trim();
        ConceptoNombre = NullIfBlank(conceptoNombre);
        ProveedorNombre = NullIfBlank(proveedorNombre);
        FormaPagoNombre = NullIfBlank(formaPagoNombre);
        Prioridad = prioridad;
        Activo = activo;

        return Result.Success();
    }

    private static Result Validar(
        string patron,
        string? tipo,
        string categoriaNombre,
        string? conceptoNombre,
        string? proveedorNombre,
        string? formaPagoNombre)
    {
        if (string.IsNullOrWhiteSpace(patron))
        {
            return Result.Failure(Error.Validation("El patrón de búsqueda es obligatorio."));
        }

        if (patron.Trim().Length > PatronMaxLength)
        {
            return Result.Failure(Error.Validation($"El patrón no puede exceder los {PatronMaxLength} caracteres."));
        }

        if (string.IsNullOrWhiteSpace(categoriaNombre))
        {
            return Result.Failure(Error.Validation("La categoría es obligatoria."));
        }

        if (categoriaNombre.Trim().Length > NombreMaxLength
            || (conceptoNombre?.Trim().Length ?? 0) > NombreMaxLength
            || (proveedorNombre?.Trim().Length ?? 0) > NombreMaxLength
            || (formaPagoNombre?.Trim().Length ?? 0) > NombreMaxLength)
        {
            return Result.Failure(Error.Validation($"Ninguno de los nombres puede exceder los {NombreMaxLength} caracteres."));
        }

        if (!string.IsNullOrWhiteSpace(tipo)
            && !tipo.Equals("gasto", StringComparison.OrdinalIgnoreCase)
            && !tipo.Equals("ingreso", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(Error.Validation("El tipo debe ser 'gasto', 'ingreso' o vacío (cualquiera)."));
        }

        return Result.Success();
    }

    private static string? NormalizarTipo(string? tipo)
        => string.IsNullOrWhiteSpace(tipo) ? null : tipo.Trim().ToLowerInvariant();

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
