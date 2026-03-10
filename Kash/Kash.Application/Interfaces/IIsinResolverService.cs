namespace Kash.Application.Interfaces;

public interface IIsinResolverService
{
    Task<string> ResolveAsync(string isin, CancellationToken cancellationToken = default);
}
