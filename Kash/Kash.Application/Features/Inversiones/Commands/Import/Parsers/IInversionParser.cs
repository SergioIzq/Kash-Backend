using Kash.Application.Features.Inversiones.Commands.Import.Models;

namespace Kash.Application.Features.Inversiones.Commands.Import.Parsers;

public interface IInversionParser
{
    Task<ParseResult> ParseAsync(byte[] content, CancellationToken cancellationToken = default);
}
