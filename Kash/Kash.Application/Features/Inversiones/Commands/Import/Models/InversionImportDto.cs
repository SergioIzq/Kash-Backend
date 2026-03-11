namespace Kash.Application.Features.Inversiones.Commands.Import.Models;

public sealed record InversionImportDto(
    string Nombre,
    string Ticker,
    string Tipo,
    decimal Cantidad,
    decimal PrecioCompra,
    string Moneda,
    DateOnly FechaCompra,
    string? Descripcion,
    string? Plataforma);
