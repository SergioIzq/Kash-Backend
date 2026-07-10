using Kash.Domain;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kash.Infrastructure.Services.Auth;

/// <summary>
/// Implementación del generador de tokens JWT.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(Usuario usuario)
    {
        // 1. Obtener configuración
        var jwtKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey no está configurada.");
        var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "Kash";
        var jwtAudience = _configuration["JwtSettings:Audience"] ?? "Kash";
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "720");

        // 2. Crear claims
        var expirationTime = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new[]
   {
         new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo.Value),
       new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
   new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
        };

        // 3. Crear credenciales de firma
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. Crear el token
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
    claims: claims,
            expires: expirationTime,
         signingCredentials: creds
        );

        // 5. Retornar el token serializado
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expirationTime);
    }
}
