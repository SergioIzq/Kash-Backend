using AhorroLand.Domain.TraspasosProgramados.Eventos;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Domain;

public sealed class TraspasoProgramado : AbsEntity
{
    private TraspasoProgramado() : base(Guid.Empty)
    {

    }

    private TraspasoProgramado(
        Guid id,
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

    // --- Propiedades derivadas (opcional: para proyecciones) ---
    public Cantidad SaldoCuentaOrigen { get; private set; }
    public Cantidad SaldoCuentaDestino { get; private set; }

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
            Guid.NewGuid(),
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
            traspaso.Id,
            frecuencia,
            fechaEjecucion
        ));

        return Result.Success(traspaso);
    }


    // --- Comportamientos del dominio ---

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
    /// Actualiza el identificador del job de Hangfire.
    /// </summary>
    public Result AsignarJobId(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return Result.Failure(Error.Validation("El Job ID no puede ser vacío."));

        HangfireJobId = jobId;
        return Result.Success();
    }

    /// <summary>
    /// Actualiza los saldos de las cuentas (solo informativo, no cambia el dominio de Cuentas).
    /// </summary>
    public void ActualizarSaldos(Cantidad saldoOrigen, Cantidad saldoDestino)
    {
        SaldoCuentaOrigen = saldoOrigen;
        SaldoCuentaDestino = saldoDestino;
    }
}
