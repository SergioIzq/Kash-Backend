namespace Kash.Infrastructure.Configuration.Settings
{
    // Define esta clave estática para facilitar la lectura desde appsettings.json
    public static class SettingsConstants
    {
        public const string EmailSettings = "EmailSettings";
    }

    public class EmailSettings
    {
        // Propiedad que mapea la sección de configuración
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPass { get; set; } = string.Empty;

        // Propiedades adicionales para compatibilidad con EmailSenderService
        public string Username => SmtpUser;
        public string Password => SmtpPass;
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = "notificaciones@sergioizq.com";
        public string FromName { get; set; } = "Equipo Kash";
    }
}
