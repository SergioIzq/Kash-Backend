using Kash.Infrastructure.Configuration.Settings;
using Kash.Shared.Application.Dtos;
using SergioIzq.Application.Kernel.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Kash.Infrastructure.Services;

/// <summary>
/// Servicio en segundo plano que procesa la cola de emails.
/// Utiliza MailKit para envío asíncrono y eficiente de correos electrónicos.
/// </summary>
public class EmailBackgroundSender : BackgroundService
{
    private readonly QueuedEmailService _emailQueue;
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailBackgroundSender> _logger;

    public EmailBackgroundSender(
    QueuedEmailService emailQueue,
IOptions<EmailSettings> options,
        ILogger<EmailBackgroundSender> logger)
    {
        _emailQueue = emailQueue;
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("📧 Servicio de envío de emails en segundo plano iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_emailQueue.TryDequeue(out var message))
            {
                await SendEmailInternalAsync(message!, stoppingToken);
            }
            else
            {
                // Esperamos un momento si la cola está vacía
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("📧 Servicio de envío de emails en segundo plano detenido");
    }

    private async Task SendEmailInternalAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();

        try
        {
            // 1. Crear el mensaje MIME
            var mimeMessage = new MimeMessage();

            // CORRECCIÓN 1: Usar FromEmail en vez de SmtpUser
            mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            mimeMessage.ReplyTo.Add(MailboxAddress.Parse("sergioizq.dev@gmail.com"));
            mimeMessage.To.Add(MailboxAddress.Parse(message.To));
            mimeMessage.Subject = message.Subject;

            var builder = new BodyBuilder { HtmlBody = message.Body };
            mimeMessage.Body = builder.ToMessageBody();

            // 2. Conexión Inteligente
            // MailKit recomienda 'Auto' para que él detecte si debe usar SSL o StartTLS según el puerto.
            // Para Brevo puerto 587, esto usará StartTls automáticamente.
            await client.ConnectAsync(
                _settings.SmtpServer,
                _settings.SmtpPort,
                SecureSocketOptions.Auto,
                cancellationToken);

            // Autenticación
            if (!string.IsNullOrEmpty(_settings.SmtpUser) && !string.IsNullOrEmpty(_settings.SmtpPass))
            {
                await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass, cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);

            _logger.LogInformation("✅ Email enviado a {To}", message.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error enviando a {To}: {Message}", message.To, ex.Message);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
