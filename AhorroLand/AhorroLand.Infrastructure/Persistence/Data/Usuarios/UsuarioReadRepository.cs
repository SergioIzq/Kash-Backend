using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Dapper;

namespace AhorroLand.Infrastructure.Persistence.Data.Usuarios;

public sealed class UsuarioReadRepository : AbsReadRepository<Usuario, UsuarioDto, UsuarioId>, IUsuarioReadRepository
{
    public UsuarioReadRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory, "usuarios")
    {
    }

    public async Task<Usuario?> GetByEmailAsync(Email correo, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, correo, contrasena as Contrasena, token_confirmacion as TokenConfirmacion, activo
             FROM usuarios
            WHERE correo = @Correo
            LIMIT 1";

        var connection = _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<UsuarioDataModel>(sql, new { Correo = correo.Value });

        return result != null ? MapToEntity(result) : null;
    }

    public async Task<Usuario?> GetByConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, correo, contrasena as Contrasena, token_confirmacion as TokenConfirmacion, activo
                    FROM usuarios
             WHERE token_confirmacion = @Token
            LIMIT 1";

        var connection = _dbConnectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<UsuarioDataModel>(sql, new { Token = token });

        return result != null ? MapToEntity(result) : null;
    }

    private Usuario MapToEntity(UsuarioDataModel dto)
    {
        // Usar reflection para crear la entidad con constructor privado
        var constructor = typeof(Usuario).GetConstructor(
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new[] { typeof(UsuarioId), typeof(Email), typeof(PasswordHash), typeof(ConfirmationToken?), typeof(bool) },
            null);

        var usuarioId = new UsuarioId(dto.Id); // <--- AQUÍ ESTABA EL ERROR
        var email = new Email(dto.Correo);
        var passwordHash = new PasswordHash(dto.Contrasena);

        // Asumiendo que ConfirmationToken es un struct o class con constructor que recibe string
        ConfirmationToken? token = dto.TokenConfirmacion != null
            ? new ConfirmationToken(dto.TokenConfirmacion)
            : null;

        // 2. Pasar los objetos tipados al constructor
        return (Usuario)constructor!.Invoke([
            usuarioId,    // Ahora pasas un UsuarioId, no un Guid
            email,
            passwordHash,
            token,
            dto.Activo
        ]);
    }

    private class UsuarioDataModel
    {
        public Guid Id { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string? TokenConfirmacion { get; set; }
        public bool Activo { get; set; }
    }
}