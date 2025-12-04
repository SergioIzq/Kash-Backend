using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("conceptos")]
public sealed class Concepto : AbsEntity<ConceptoId>
{
    // Constructor privado sin parámetros para EF Core
    private Concepto() : base(ConceptoId.Create(Guid.Empty).Value)
    {
    }

    private Concepto(ConceptoId id, Nombre nombre, CategoriaId categoriaId, UsuarioId usuarioId) : base(id)
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
        var concepto = new Concepto(ConceptoId.Create(Guid.NewGuid()).Value, nombre, categoriaId, usuarioId);

        return concepto;
    }

    public void Update(Nombre nombre, CategoriaId categoriaId)
    {
        Nombre = nombre;
        CategoriaId = categoriaId;
    }
}