using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("cuentas")]
public sealed class Cuenta : AbsEntity<CuentaId>
{
    // Constructor privado sin parámetros para EF Core
    private Cuenta() : base(CuentaId.Create(Guid.NewGuid()).Value)
    {
    }

    private Cuenta(CuentaId id, Nombre nombre, Cantidad saldo, UsuarioId usuarioId) : base(id)
    {
        Nombre = nombre;
        Saldo = saldo;
        UsuarioId = usuarioId;
    }

    public Nombre Nombre { get; private set; }
    public Cantidad Saldo { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    public static Cuenta Create(Nombre nombre, Cantidad saldo, UsuarioId usuarioId)
    {
        var cuenta = new Cuenta(CuentaId.Create(Guid.NewGuid()).Value, nombre, saldo, usuarioId);

        return cuenta;
    }

    public void Update(Nombre nombre) => Nombre = nombre;

    /// <summary>
    /// Deposita una cantidad en la cuenta, aumentando el saldo.
    /// </summary>
    public void Depositar(Cantidad cantidad)
    {
        Saldo = Saldo.Sumar(cantidad);
    }

    /// <summary>
    /// Retira una cantidad de la cuenta, disminuyendo el saldo, si hay fondos suficientes.
    /// </summary>
    public void Retirar(Cantidad cantidad)
    {
        Saldo = Saldo.Restar(cantidad);
    }
}
