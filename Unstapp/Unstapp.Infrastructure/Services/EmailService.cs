using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Unstapp.Shared.Interfaces;

namespace Unstapp.Infrastructure.Services
{
    /// <summary>
    /// Servicio de mensajería. Si hay un host SMTP configurado en "Email:Smtp:Host"
    /// envía el correo real; de lo contrario (entorno de desarrollo / sin credenciales)
    /// registra el enlace en el log para poder seguir el flujo sin un servidor SMTP.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendFirstLoginEmailAsync(string toEmail, string fullName, string confirmationLink)
        {
            var fromAddress = _config["Email:From"] ?? "no-reply@unstapp.edu";
            var fromName = _config["Email:FromName"] ?? "Unstapp";
            const string subject = "Activá tu cuenta en Unstapp";

            var body =
                $"<p>Hola {WebUtility.HtmlEncode(fullName)},</p>" +
                "<p>Para completar el nuevo inicio de sesión hacé click aquí:</p>" +
                $"<p><a href=\"{confirmationLink}\">Crear mi contraseña</a></p>" +
                "<p>Este enlace vence en 10 minutos. Si no solicitaste el acceso, ignorá este mensaje.</p>";

            var smtpHost = _config["Email:Smtp:Host"];

            // Modo desarrollo: sin SMTP configurado, dejamos traza del enlace en el log.
            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                _logger.LogInformation(
                    "[EmailService] (modo dev, sin SMTP) Correo de primer login para {Email}. Enlace: {Link}",
                    toEmail, confirmationLink);
                return;
            }

            var port = int.TryParse(_config["Email:Smtp:Port"], out var p) ? p : 587;
            var enableSsl = !bool.TryParse(_config["Email:Smtp:EnableSsl"], out var ssl) || ssl;
            var username = _config["Email:Smtp:Username"];
            var password = _config["Email:Smtp:Password"];

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(smtpHost, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            await client.SendMailAsync(message);

            _logger.LogInformation("[EmailService] Correo de primer login enviado a {Email}.", toEmail);
        }
    }
}
