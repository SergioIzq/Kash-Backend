using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("categorias")]
public sealed class Categoria : AbsEntity<CategoriaId>
{
    // Constructor privado sin parámetros para EF Core
    private Categoria() : base(CategoriaId.Create(Guid.NewGuid()).Value)
    {
        // EF Core usará este constructor y luego establecerá las propiedades
    }

    private Categoria(CategoriaId id, Nombre nombre, UsuarioId idUsuario, Descripcion? descripcion = null) : base(id)
    {
        Nombre = nombre;
        IdUsuario = idUsuario;
        Descripcion = descripcion;
    }

    public Nombre Nombre { get; private set; }
    public Descripcion? Descripcion { get; private set; }
    public UsuarioId IdUsuario { get; private set; }

    public static Categoria Create(Nombre nombre, UsuarioId usuarioId, Descripcion? descripcion = null)
    {
        var categoria = new Categoria(CategoriaId.Create(Guid.NewGuid()).Value, nombre, usuarioId, descripcion);

        return categoria;
    }

    /// <summary>
    /// Actualiza el nombre y la descripción de la categoría.
    /// </summary>
    /// <param name="nombre">El nuevo Value Object Nombre (ya validado).</param>
    /// <param name="descripcion">El nuevo Value Object Descripcion (opcional).</param>
    public void Update(Nombre nombre, Descripcion? descripcion) => (Nombre, Descripcion) = (nombre, descripcion);
}
