using System.Security.Cryptography;
using System.Text;
using Kash.Shared.Application.Interfaces;

namespace Kash.Infrastructure.Services.Auth;

public sealed class ApiTokenHasher : IApiTokenHasher
{
    private const string Prefix = "kash_pat_";

    public (string PlainToken, string Hash) GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32); // 256 bits
        var plainToken = Prefix + Base64UrlEncode(randomBytes);

        return (plainToken, Hash(plainToken));
    }

    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
