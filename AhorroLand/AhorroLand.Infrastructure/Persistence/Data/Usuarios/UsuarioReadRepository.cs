using AhorroLand.Domain;
using AhorroLand.Infrastructure.Persistence.Query;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using Dapper;
using System.Reflection;

namespace AhorroLand.Infrastructure.Persistence.Data.Usuarios;

public sealed class UsuarioReadRepository : AbsReadRepository<Usuario, UsuarioDto, UsuarioId>, IUsuarioReadRepository
{
    // Cacheamos el constructor para no usar Reflection lento en cada llamada
    private static readonly ConstructorInfo UsuarioConstructor = typeof(Usuario).GetConstructor(
        BindingFlags.NonPublic | BindingFlags.Instance,
        null,
        new[] {
            typeof(UsuarioId), typeof(Email), typeof(PasswordHash), typeof(ConfirmationToken?),
            typeof(bool), typeof(ConfirmationToken?), typeof(DateTime?), typeof(Nombre?), typeof(Apellido?)
        },
        null)!;

    public UsuarioReadRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory, "usuarios")
    {
    }

    protected override string BuildGetByIdQuery()
    {
        return @"
          SELECT 
            id as Id,
            correo as Correo,
            nombre as Nombre,
            apellidos as Apellidos,
            fecha_creacion as FechaCreacion
          FROM usuarios 
          WHERE id = @id";
    }

    public async Task<Usuario?> GetByEmailAsync(Email correo, CancellationToken cancellationToken = default)
    {
        // Seleccionamos las columnas primitivas
        const string sql = @"
            SELECT 
                id, 
                correo, 
                nombre, 
                apellidos, 
                contrasena, 
                token_confirmacion, 
                activo, 
                token_recuperacion, 
                token_recuperacion_expiracion
            FROM usuarios
            WHERE correo = @Correo
            LIMIT 1";

        using var connection = _dbConnectionFactory.CreateConnection();

        // Usamos dynamic para evitar la clase UsuarioDataModel
        var row = await connection.QueryFirstOrDefaultAsync(sql, new { Correo = correo.Value });

        if (row == null) return null;

        return MapRowToUsuario(row);
    }

    public async Task<Usuario?> GetByConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id, 
                correo, 
                nombre, 
                apellidos, 
                contrasena, 
                token_confirmacion, 
                activo, 
                token_recuperacion, 
                token_recuperacion_expiracion
            FROM usuarios
            WHERE token_confirmacion = @Token
            LIMIT 1";

        using var connection = _dbConnectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync(sql, new { Token = token });

        if (row == null) return null;

        return MapRowToUsuario(row);
    }

    // Método helper limpio que convierte dynamic (BD) -> Dominio
    private Usuario MapRowToUsuario(dynamic row)
    {
        var id = UsuarioId.Create(Guid.Parse(row.id.ToString())).Value;
        var email = Email.Create((string)row.correo).Value;
        var password = PasswordHash.Create((string)row.contrasena).Value;

        // 🔧 CORRECCIÓN 1: Cast explícito a (ConfirmationToken?)null
        // C# necesita saber qué tipo de "null" es para el operador ternario.
        var tokenConf = row.token_confirmacion != null
            ? ConfirmationToken.Create((string)row.token_confirmacion).Value
            : (ConfirmationToken?)null;

        var tokenRecup = row.token_recuperacion != null
            ? ConfirmationToken.Create((string)row.token_recuperacion).Value
            : (ConfirmationToken?)null;

        DateTime? tokenExp = row.token_recuperacion_expiracion != null
            ? (DateTime)row.token_recuperacion_expiracion
            : null;

        var nombre = row.nombre != null
            ? Nombre.CreateFromDatabase((string)row.nombre)
            : (Nombre?)null;

        // 2. Cast explícito para Apellido
        var apellidos = row.apellidos != null
            ? Apellido.CreateFromDatabase((string)row.apellidos)
            : (Apellido?)null;

        // Invocamos el constructor privado
        return (Usuario)UsuarioConstructor.Invoke(new object?[] {
        id,
        email,
        password,
        tokenConf,
        (bool)row.activo,
        tokenRecup,
        tokenExp,
        nombre,
        apellidos
    });
    }
}