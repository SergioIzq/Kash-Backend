namespace Kash.Shared.Application.Dtos;

/// <summary>
/// DTO plano que Dapper mapea directamente desde la tabla inversiones.
/// Tipo se almacena como string lowercase ("etf", "accion", "mercado_privado", etc.)
/// </summary>
public record InversionDto
{
    public Guid     Id          { get; init; }
    public string   Nombre      { get; init; } = string.Empty;
    public string   Ticker      { get; init; } = string.Empty;
    public string   Tipo        { get; init; } = string.Empty;
    public decimal  Cantidad    { get; init; }
    public decimal  PrecioCompra { get; init; }
    public string   Moneda      { get; init; } = string.Empty;
    public DateTime FechaCompra { get; init; }
    public string?  Descripcion { get; init; }
    public string?  Plataforma  { get; init; }
    public Guid     UsuarioId   { get; init; }
}
