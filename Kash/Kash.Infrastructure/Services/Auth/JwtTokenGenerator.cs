using Kash.Domain;
using Kash.Shared.Application.Interfaces;
using SergioIzq.AspNetCore.Kernel.Auth;

namespace Kash.Infrastructure.Services.Auth;

/// <summary>
/// Adapter fino de <see cref="IJwtTokenGenerator"/> (que recibe la entidad Usuario de Kash)
/// sobre el generador genérico del kernel.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly KernelJwtTokenGenerator _kernelGenerator;

    public JwtTokenGenerator(KernelJwtTokenGenerator kernelGenerator)
    {
        _kernelGenerator = kernelGenerator;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(Usuario usuario)
    {
        return _kernelGenerator.GenerateToken(usuario.Id.Value, usuario.Correo.Value);
    }
}
