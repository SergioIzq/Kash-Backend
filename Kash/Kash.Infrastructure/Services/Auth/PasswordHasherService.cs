using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Kash.Infrastructure.Services.Auth;

/// <summary>
/// Implementación del servicio de hashing de contraseñas usando ASP.NET Core Identity.
/// </summary>
public sealed class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));
        }

        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new ArgumentException("El hash no puede estar vacío.", nameof(hashedPassword));
        }

        var result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword, password);
        return result == PasswordVerificationResult.Success;
    }
}
