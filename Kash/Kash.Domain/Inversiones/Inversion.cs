using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kash.Domain;

[Table("inversiones")]
public sealed class Inversion : AbsEntity<InversionId>
{
    private Inversion() : base(InversionId.CreateFromDatabase(Guid.NewGuid()))
    {
    }

    private Inversion(
        InversionId id,
        string nombre,
        string ticker,
        TipoInversion tipo,
        decimal cantidad,
        decimal precioCompra,
        string moneda,
        DateTime fechaCompra,
        UsuarioId usuarioId,
        string? descripcion,
        string? plataforma) : base(id)
    {
        Nombre = nombre;
        Ticker = ticker;
        Tipo = tipo;
        Cantidad = cantidad;
        PrecioCompra = precioCompra;
        Moneda = moneda;
        FechaCompra = fechaCompra;
        UsuarioId = usuarioId;
        Descripcion = descripcion;
        Plataforma = plataforma;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Ticker { get; private set; } = string.Empty;
    public TipoInversion Tipo { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal PrecioCompra { get; private set; }
    public string Moneda { get; private set; } = string.Empty;
    public DateTime FechaCompra { get; private set; }
    public string? Descripcion { get; private set; }
    public string? Plataforma { get; private set; }
    public UsuarioId UsuarioId { get; private set; }
    public DateTime? ActualizadoEn { get; private set; }

    // ──────────────────────────────────────────────────────────
    // Factory
    // ──────────────────────────────────────────────────────────

    public static Result<Inversion> Create(
        string nombre,
        string ticker,
        string tipoStr,
        decimal cantidad,
        decimal precioCompra,
        string moneda,
        DateTime fechaCompra,
        UsuarioId usuarioId,
        string? descripcion,
        string? plataforma)
    {
        var validationError = ValidateInvariants(nombre, ticker, tipoStr, cantidad, precioCompra, moneda, fechaCompra);
        if (validationError is not null)
            return Result.Failure<Inversion>(validationError);

        var tipo = TipoInversionConverter.FromDb(tipoStr);

        // Ticker obligatorio para todos los tipos salvo MercadoPrivado
        if (tipo != TipoInversion.MercadoPrivado && string.IsNullOrWhiteSpace(ticker))
            return Result.Failure<Inversion>(InversionErrors.TickerRequerido);

        return Result.Success(new Inversion(
            InversionId.CreateFromDatabase(Guid.NewGuid()),
            nombre.Trim(),
            ticker.Trim(),
            tipo,
            cantidad,
            precioCompra,
            moneda.Trim().ToUpperInvariant(),
            fechaCompra.Date,
            usuarioId,
            descripcion?.Trim(),
            plataforma?.Trim()));
    }

    // ──────────────────────────────────────────────────────────
    // Update
    // ──────────────────────────────────────────────────────────

    public Result Update(
        string nombre,
        string ticker,
        string tipoStr,
        decimal cantidad,
        decimal precioCompra,
        string moneda,
        DateTime fechaCompra,
        string? descripcion,
        string? plataforma)
    {
        var validationError = ValidateInvariants(nombre, ticker, tipoStr, cantidad, precioCompra, moneda, fechaCompra);
        if (validationError is not null)
            return Result.Failure(validationError);

        var tipo = TipoInversionConverter.FromDb(tipoStr);

        if (tipo != TipoInversion.MercadoPrivado && string.IsNullOrWhiteSpace(ticker))
            return Result.Failure(InversionErrors.TickerRequerido);

        Nombre = nombre.Trim();
        Ticker = ticker.Trim();
        Tipo = tipo;
        Cantidad = cantidad;
        PrecioCompra = precioCompra;
        Moneda = moneda.Trim().ToUpperInvariant();
        FechaCompra = fechaCompra.Date;
        Descripcion = descripcion?.Trim();
        Plataforma = plataforma?.Trim();
        ActualizadoEn = DateTime.UtcNow;

        return Result.Success();
    }

    // ──────────────────────────────────────────────────────────
    // Invariants
    // ──────────────────────────────────────────────────────────

    private static Error? ValidateInvariants(
        string nombre,
        string ticker,
        string tipoStr,
        decimal cantidad,
        decimal precioCompra,
        string moneda,
        DateTime fechaCompra)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Error.Validation("El nombre de la inversión es obligatorio.");

        if (nombre.Length > 200)
            return Error.Validation("El nombre no puede superar 200 caracteres.");

        if (!TipoInversionConverter.IsValid(tipoStr))
            return Error.Validation($"Tipo de inversión inválido. Valores permitidos: {string.Join(", ", TipoInversionConverter.ValidValues)}.");

        if (ticker.Length > 20)
            return Error.Validation("El ticker no puede superar 20 caracteres.");

        if (cantidad <= 0)
            return Error.Validation("La cantidad debe ser mayor que 0.");

        if (precioCompra <= 0)
            return Error.Validation("El precio de compra debe ser mayor que 0.");

        if (string.IsNullOrWhiteSpace(moneda) || moneda.Length != 3 || !moneda.All(char.IsLetter))
            return Error.Validation("La moneda debe ser un código ISO de 3 letras (ej: USD, EUR).");

        if (fechaCompra.Date > DateTime.Today)
            return Error.Validation("La fecha de compra no puede ser futura.");

        return null;
    }
}
