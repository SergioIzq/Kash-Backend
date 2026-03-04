using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Ingresos.Commands;

/// <summary>
/// Comando para crear un ingreso con soporte de auto-creación de entidades relacionadas.
/// Si se proporciona un nombre y el ID no existe, se creará la entidad automáticamente.
/// </summary>
public sealed record CreateIngresoCommand : AbsCreateCommand<Ingreso, IngresoId>
{
    public required decimal Importe { get; init; }
    public required DateTime Fecha { get; init; }
    public string? Descripcion { get; init; }

    // IDs de entidades relacionadas
    public required Guid CategoriaId { get; init; }
    public required Guid ConceptoId { get; init; }
    public required Guid? ClienteId { get; init; }
    public required Guid? PersonaId { get; init; }
    public required Guid CuentaId { get; init; }
    public required Guid FormaPagoId { get; init; }
    public required Guid UsuarioId { get; init; }

    // ?? Nombres opcionales para auto-creación
    /// <summary>
    /// Nombre de la categoría. Si CategoriaId no existe y se proporciona este valor,
    /// se creará automáticamente la categoría.
    /// </summary>
    public string? CategoriaNombre { get; init; }

    /// <summary>
    /// Nombre del concepto. Si ConceptoId no existe y se proporciona este valor,
    /// se creará automáticamente el concepto (con la categoría especificada o creada).
    /// </summary>
    public string? ConceptoNombre { get; init; }

    /// <summary>
    /// Nombre del cliente. Si ClienteId no existe y se proporciona este valor,
    /// se creará automáticamente el cliente.
    /// </summary>
    public string? ClienteNombre { get; init; }

    /// <summary>
    /// Nombre de la persona. Si PersonaId no existe y se proporciona este valor,
    /// se creará automáticamente la persona.
    /// </summary>
    public string? PersonaNombre { get; init; }

    /// <summary>
    /// Nombre de la cuenta. Si CuentaId no existe y se proporciona este valor,
    /// se creará automáticamente la cuenta.
    /// </summary>
    public string? CuentaNombre { get; init; }

    /// <summary>
    /// Nombre de la forma de pago. Si FormaPagoId no existe y se proporciona este valor,
    /// se creará automáticamente la forma de pago.
    /// </summary>
    public string? FormaPagoNombre { get; init; }
}