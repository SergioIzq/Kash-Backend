using AhorroLand.Domain.GastosProgramados.Eventos;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhorroLand.Domain;

[Table("gastos_programados")]
public sealed class GastoProgramado : AbsEntity<GastoProgramadoId>
{
    private GastoProgramado() : base(GastoProgramadoId.Create(Guid.Empty).Value)
    {
    }

    private GastoProgramado(
        GastoProgramadoId id,
        Cantidad importe,
        DateTime fechaEjecucion,
        ConceptoId conceptoId,
        ProveedorId proveedorId,
        PersonaId personaId,
        CuentaId cuentaId,
        FormaPagoId formaPagoId,
        Frecuencia frecuencia,
        string hangfireJobId,
        Descripcion? descripcion = null) : base(id)
    {
        Importe = importe;
        FechaEjecucion = fechaEjecucion;

        ConceptoId = conceptoId;
        ProveedorId = proveedorId;
        PersonaId = personaId;
        CuentaId = cuentaId;
        FormaPagoId = formaPagoId;

        Frecuencia = frecuencia;
        Descripcion = descripcion;
        Activo = true;
        HangfireJobId = hangfireJobId;
    }

    public Cantidad Importe { get; private set; }
    public Frecuencia Frecuencia { get; private set; }
    public Descripcion? Descripcion { get; private set; }
    public DateTime FechaEjecucion { get; private set; }
    public bool Activo { get; private set; }
    public ConceptoId ConceptoId { get; private set; }
    public ProveedorId ProveedorId { get; private set; }
    public PersonaId PersonaId { get; private set; }
    public CuentaId CuentaId { get; private set; }
    public UsuarioId UsuarioId { get; private set; }
    public FormaPagoId FormaPagoId { get; private set; }
    public string HangfireJobId { get; private set; } = string.Empty;

    public static GastoProgramado Create(
        Cantidad importe,
        DateTime fechaEjecucion,
        ConceptoId conceptoId,
        ProveedorId proveedorId,
        Frecuencia frecuencia,
        PersonaId personaId,
        CuentaId cuentaId,
        FormaPagoId formaPagoId,
        string hangfireJobId,
        Descripcion? descripcion = null)
    {
        var gasto = new GastoProgramado(
            GastoProgramadoId.Create(Guid.NewGuid()).Value,
            importe,
            fechaEjecucion,
            conceptoId,
            proveedorId,
            personaId,
            cuentaId,
            formaPagoId,
            frecuencia,
            hangfireJobId,
            descripcion);

        // 🔥 LANZAR EVENTO DE DOMINIO
        gasto.AddDomainEvent(new GastoProgramadoCreadoEvent(
            gasto.Id.Value,
            frecuencia,
            fechaEjecucion
        ));

        return gasto;
    }

    /// <summary>
    /// Cambia los detalles de la programación.
    /// </summary>
    public Result Reprogramar(DateTime nuevaFecha, Frecuencia nuevaFrecuencia)
    {
        // ⭐ Aplica reglas de negocio aquí (ej: la nueva fecha debe ser futura)
        if (nuevaFecha < DateTime.Today)
        {
            return Result.Failure(Error.Validation("La reprogramación debe ser para una fecha futura."));
        }
        FechaEjecucion = nuevaFecha;
        Frecuencia = nuevaFrecuencia;

        return Result.Success();
    }

    /// <summary>
    /// Marca el Gasto como inactivo.
    /// </summary>
    public void Desactivar()
    {
        // ⭐ Aplica reglas de negocio aquí (ej: solo si no está ya ejecutado)
        Activo = false;
        // Opcional: Levantar un evento de dominio
    }

    /// <summary>
    /// Asigna el ID del Job de Hangfire después de la creación/actualización.
    /// </summary>
    public Result AsignarJobId(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return Result.Failure(Error.Validation("El Job ID no puede ser vacío."));
        }
        HangfireJobId = jobId;

        return Result.Success();
    }
}