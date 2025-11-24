using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain;

public sealed class Categoria : AbsEntity
{
    // Constructor privado sin parámetros para EF Core
    private Categoria() : base(Guid.Empty)
    {
        // EF Core usará este constructor y luego establecerá las propiedades
    }

    private Categoria(Guid id, Nombre nombre, UsuarioId idUsuario, Descripcion? descripcion = null) : base(id)
    {
        Nombre = nombre;
        IdUsuario = idUsuario;
        Descripcion = descripcion;
    }

    public Nombre Nombre { get; private set; }
    // ⚠️ NOTA: Descripcion no está mapeada en la BD, solo se usa en memoria
    public Descripcion? Descripcion { get; private set; }
    public UsuarioId IdUsuario { get; private set; }

    public static Categoria Create(Nombre nombre, UsuarioId usuarioId, Descripcion? descripcion = null)
    {
        var categoria = new Categoria(Guid.NewGuid(), nombre, usuarioId, descripcion); // Descripcion puede ser null

        return categoria;
    }

    /// <summary>
    /// Actualiza el nombre y la descripción de la categoría.
    /// </summary>
    /// <param name="nombre">El nuevo Value Object Nombre (ya validado).</param>
    /// <param name="descripcion">El nuevo Value Object Descripcion (opcional).</param>
    public void Update(Nombre nombre, Descripcion? descripcion) => (Nombre, Descripcion) = (nombre, descripcion);
}
