namespace Kash.Application.Features.Inversiones.Commands.Import.Models;

public sealed record ImportErrorLinea(int Linea, string Contenido, string Razon);
