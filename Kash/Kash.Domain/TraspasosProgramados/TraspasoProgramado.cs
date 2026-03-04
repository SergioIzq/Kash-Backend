using Kash.Domain.TraspasosProgramados.Eventos;
using Kash.Shared.Domain.Abstractions;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kash.Domain;

[Table("traspasos_programados")]
public sealed class TraspasoProgramado : AbsEntity<TraspasoProgramadoId>
{
    private TraspasoProgramado() : base(TraspasoProgramadoId.Create(Guid.NewGuid()).Value)
    {

    }

    private TraspasoProgramado(
        TraspasoProgramadoId id,
        CuentaId cuentaOrigenId,
        CuentaId cuentaDestinoId,
        Cantidad importe,
        DateTime fechaEjecucion,
        Frecuencia frecuencia,
        string hangfireJobId,
        UsuarioId usuarioId,
        Descripcion? descripcion = null) : base(id)
    {
        CuentaOrigenId = cuentaOrigenId;
        CuentaDestinoId = cuentaDestinoId;
        Importe = importe;
        FechaEjecucion = fechaEjecucion;
        Frecuencia = frecuencia;
        HangfireJobId = hangfireJobId;
        UsuarioId = usuarioId;
        Descripcion = descripcion;
        Activo = true;
    }

    // --- Value Objects ---
    public CuentaId CuentaOrigenId { get; private set; }
    public CuentaId CuentaDestinoId { get; private set; }
    public Cantidad Importe { get; private set; }
    public Frecuencia Frecuencia { get; private set; }
    public Descripcion? Descripcion { get; private set; }
    public UsuarioId UsuarioId { get; private set; }

    // --- Atributos simples ---
    public DateTime FechaEjecucion { get; private set; }
    public bool Activo { get; private set; }
    public string HangfireJobId { get; private set; } = string.Empty;

    public Cuenta CuentaOrigen { get; private set; } = null!;
    public Cuenta CuentaDestino { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    // --- Factory method ---
    public static Result<TraspasoProgramado> Create(
    CuentaId cuentaOrigenId,
    CuentaId cuentaDestinoId,
    Cantidad importe,
    DateTime fechaEjecucion,
    Frecuencia frecuencia,
    UsuarioId usuarioId,
    string hangfireJobId,
    Descripcion? descripcion = null)
    {
        if (cuentaOrigenId == cuentaDestinoId)
            return Result.Failure<TraspasoProgramado>(Error.Validation("La cuenta de origen y destino no pueden ser la misma."));

        if (importe.Valor <= 0)
            return Result.Failure<TraspasoProgramado>(Error.Validation("El importe debe ser mayor a cero."));

        var traspaso = new TraspasoProgramado(
            TraspasoProgramadoId.Create(Guid.NewGuid()).Value,
            cuentaOrigenId,
            cuentaDestinoId,
            importe,
            fechaEjecucion,
            frecuencia,
            hangfireJobId,
            usuarioId,
            descripcion
        );

        // 🔥 LANZAR EVENTO DE DOMINIO
        traspaso.AddDomainEvent(new TraspasoProgramadoCreadoEvent(
            traspaso.Id.Value,
            frecuencia,
            fechaEjecucion
        ));

        return Result.Success(traspaso);
    }


    // --- Comportamientos del dominio ---

    /// <summary>
    /// Actualiza todos los datos del traspaso programado.
    /// 🔥 Dispara evento de dominio si cambian las cuentas o el importe.
    /// </summary>
    public Result Update(
        CuentaId cuentaOrigenId,
        CuentaId cuentaDestinoId,
        Cantidad importe,
        DateTime fechaEjecucion,
        Frecuencia frecuencia,
        bool activo,
        Descripcion? descripcion = null)
    {
        // Validaciones
        if (cuentaOrigenId == cuentaDestinoId)
            return Result.Failure(Error.Validation("La cuenta de origen y destino no pueden ser la misma."));

        if (importe.Valor <= 0)
            return Result.Failure(Error.Validation("El importe debe ser mayor a cero."));

        if (fechaEjecucion < DateTime.Today)
            return Result.Failure(Error.Validation("La fecha de ejecución debe ser futura."));

        // 🔥 Guardar valores anteriores para el evento
        var cuentaOrigenAnterior = CuentaOrigenId;
        var cuentaDestinoAnterior = CuentaDestinoId;
        var importeAnterior = Importe;

        // Actualizar propiedades
        CuentaOrigenId = cuentaOrigenId;
        CuentaDestinoId = cuentaDestinoId;
        Importe = importe;
        FechaEjecucion = fechaEjecucion;
        Frecuencia = frecuencia;
        Activo = activo;
        Descripcion = descripcion;

        // 🔥 Lanzar evento solo si cambió alguna cuenta o el importe
        if (!cuentaOrigenAnterior.Equals(cuentaOrigenId) ||
            !cuentaDestinoAnterior.Equals(cuentaDestinoId) ||
            !importeAnterior.Equals(importe))
        {
            AddDomainEvent(new TraspasoProgramadoActualizadoEvent(
                Id,
                cuentaOrigenAnterior,
                cuentaDestinoAnterior,
                importeAnterior,
                cuentaOrigenId,
                cuentaDestinoId,
                importe));
        }

        return Result.Success();
    }

    /// <summary>
    /// Reprograma el traspaso para una nueva fecha o frecuencia.
    /// </summary>
    public Result Reprogramar(DateTime nuevaFecha, Frecuencia nuevaFrecuencia)
    {
        if (nuevaFecha < DateTime.Today)
        {
            return Result.Failure(Error.Validation("La nueva fecha debe ser futura."));
        }

        FechaEjecucion = nuevaFecha;
        Frecuencia = nuevaFrecuencia;
        return Result.Success();
    }

    /// <summary>
    /// Desactiva el traspaso (no se ejecutará más).
    /// </summary>
    public void Desactivar()
    {
        Activo = false;
    }

    /// <summary>
    /// Marca el traspaso programado como eliminado y lanza el evento de dominio.
    /// 🔥 Dispara TraspasoProgramadoEliminadoEvent.
    /// </summary>
    public void MarkAsDeleted()
    {
        // 🔥 Lanzar evento de dominio cuando se elimina
        AddDomainEvent(new TraspasoProgramadoEliminadoEvent(
            HangfireJobId,
            Id,
            CuentaOrigenId,
            CuentaDestinoId,
            Importe));
    }

    /// <summary>
    /// Actualiza el identificador del job de Hangfire.
    /// </summary>
    public Result AsignarJobId(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return Result.Failure(Error.Validation("El Job ID no puede ser vacío."));

        HangfireJobId = jobId;
        return Result.Success();
    }
}
