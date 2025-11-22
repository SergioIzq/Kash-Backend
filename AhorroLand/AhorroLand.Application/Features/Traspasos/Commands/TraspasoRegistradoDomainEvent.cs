using AhorroLand.Application.Interfaces;
using AhorroLand.Domain;
using AhorroLand.Domain.Traspasos.Eventos;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

// ⭐ Este Event Handler maneja la lógica de actualización de saldos.
public sealed class ActualizarSaldosCuentaOnTraspasoRegistrado : IDomainEventHandler<TraspasoRegistradoDomainEvent>
{
    // ⭐ Usamos SOLO el WriteRepository para cargar entidades con tracking
    private readonly IWriteRepository<Cuenta> _cuentaWriteRepo;

    public ActualizarSaldosCuentaOnTraspasoRegistrado(
        IWriteRepository<Cuenta> cuentaWriteRepo
    )
    {
        _cuentaWriteRepo = cuentaWriteRepo;
    }

    public async Task Handle(TraspasoRegistradoDomainEvent notification, CancellationToken cancellationToken)
    {
        // 1. CARGA DE CUENTAS EN PARALELO (Máxima Optimización I/O)
        // Usamos GetByIdAsync del WriteRepository para obtener entidades con tracking
        var origenTask = _cuentaWriteRepo.GetByIdAsync(notification.CuentaOrigenId, cancellationToken);
        var destinoTask = _cuentaWriteRepo.GetByIdAsync(notification.CuentaDestinoId, cancellationToken);

        // Espera a que ambas tareas finalicen y asigna los resultados.
        var (cuentaOrigen, cuentaDestino) = await GetParallelResultsAsync(origenTask, destinoTask);

        if (cuentaOrigen is null || cuentaDestino is null)
        {
            return;
        }

        // 2. Ejecutar la lógica de negocio (Métodos de dominio)
        try
        {
            cuentaOrigen.Retirar(notification.Importe);
            cuentaDestino.Depositar(notification.Importe);

            // 3. Persistencia transaccional
            _cuentaWriteRepo.Update(cuentaOrigen);
            _cuentaWriteRepo.Update(cuentaDestino);
        }
        catch (InvalidOperationException)
        {
            // Manejo de error cuando no hay fondos suficientes
        }
    }

    // ⭐ Helper para simplificar Task.WhenAll y el manejo de resultados
    private static async Task<(T? Result1, T? Result2)> GetParallelResultsAsync<T>(
        Task<T?> task1, Task<T?> task2) where T : class
    {
        await Task.WhenAll(task1, task2);
        return (task1.Result, task2.Result);
    }
}
