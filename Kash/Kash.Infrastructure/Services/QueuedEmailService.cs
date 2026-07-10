using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using System.Collections.Concurrent;

namespace Kash.Infrastructure.Services;

/// <summary>
/// Servicio de cola para emails.
/// Utiliza ConcurrentQueue para operaciones thread-safe y eficientes.
/// </summary>
public class QueuedEmailService : IEmailService
{
    private readonly ConcurrentQueue<EmailMessage> _emailQueue = new();

    /// <summary>
    /// Encola un mensaje de email para ser procesado en segundo plano.
    /// Esta operación es extremadamente rápida y no bloquea el hilo de la API.
    /// </summary>
    public void EnqueueEmail(EmailMessage message)
    {
        _emailQueue.Enqueue(message);
    }

    /// <summary>
    /// Intenta extraer un mensaje de la cola.
    /// Método interno usado por el EmailBackgroundSender.
    /// </summary>
    internal bool TryDequeue(out EmailMessage? message)
    {
        return _emailQueue.TryDequeue(out message);
    }
}
