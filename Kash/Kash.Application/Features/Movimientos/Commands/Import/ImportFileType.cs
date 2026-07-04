namespace Kash.Application.Features.Movimientos.Commands.Import;

/// <summary>Utilidades para distinguir el tipo de fichero de extracto subido.</summary>
internal static class ImportFileType
{
    /// <summary>Detecta un PDF por su firma "%PDF" en los primeros bytes.</summary>
    public static bool EsPdf(byte[] content)
        => content.Length >= 4
           && content[0] == 0x25 && content[1] == 0x50 && content[2] == 0x44 && content[3] == 0x46;
}
