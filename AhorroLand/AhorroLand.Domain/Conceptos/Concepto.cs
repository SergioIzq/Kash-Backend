using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain;

public sealed class Concepto : AbsEntity
{
    // Constructor privado sin parámetros para EF Core
    private Concepto() : base(Guid.Empty)
    {
    }

    private Concepto(Guid id, Nombre nombre, CategoriaId categoriaId, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        CategoriaId = categoriaId;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public CategoriaId CategoriaId { get; private set; }
    public UsuarioId UsuarioId { get; private set; }
    public Categoria? Categoria { get; private set; } = null!;

    public static Concepto Create(Nombre nombre, CategoriaId categoriaId, UsuarioId usuarioId)
    {
        var concepto = new Concepto(Guid.NewGuid(), nombre, categoriaId, usuarioId);

        return concepto;
    }

    public void Update(Nombre nombre, CategoriaId categoriaId)
    {
        Nombre = nombre;
        CategoriaId = categoriaId;
    }
}