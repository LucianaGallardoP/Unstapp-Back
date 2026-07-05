using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Unstapp.Shared.Interfaces;

namespace Unstapp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendFirstLoginEmailAsync(
            string toEmail,
            string fullName,
            string confirmationLink)
        {
            var provider = _config["Email:Provider"];

            if(!string.Equals(provider, "GmailApi", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "[EmailService] Proveedor no configurado como GmailApi. Enlace para {Email}: {Link}", toEmail, confirmationLink);
                return;
            }

            var fromAddress = _config["Email:From"];
            var fromName = _config["Email:FromName"] ?? "Unstapp";

            if(string.IsNullOrWhiteSpace(fromAddress))
                throw new InvalidOperationException("La dirección de correo electrónico del remitente no está configurada en Email:From.");

            var clientId = _config["Email:GmailApi:ClientId"];
            var clientSecret = _config["Email:GmailApi:ClientSecret"];
            var refreshToken = _config["Email:GmailApi:RefreshToken"];
            var applicationName = _config["Email:GmailApi:ApplicationName"] ?? "Unstapp";

            if(string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret) ||
                string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new InvalidOperationException("Las credenciales de Gmail API no están configuradas correctamente en Email:GmailApi.");
            }

            var credential = CreateUserCredential(
                clientId,
                clientSecret,
                refreshToken);

            var gmailService = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });

            const string subject = "Activá tu cuenta en Unstapp";

            var safeFullName = WebUtility.HtmlEncode(fullName);
            var safeConfirmationLink = WebUtility.HtmlEncode(confirmationLink);

            var htmlBody =
                $"<p>Hola {safeFullName},</p>" +
                "<p>Para completar el nuevo inicio de sesión hacé click aquí:</p>" +
                $"<p><a href=\"{safeConfirmationLink}\">Crear mi contraseña</a></p>" +
                "<p>Este enlace vence en 10 minutos. Si no solicitaste el acceso, ignorá este mensaje.</p>";

            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(fromName, fromAddress));
            mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new BodyBuilder
            {
                HtmlBody = htmlBody
            }.ToMessageBody();

            var rawMessage = ConvertMimeMessageToBase64Url(mimeMessage);

            var gmailMessage = new Message
            {
                Raw = rawMessage
            };

            await gmailService.Users.Messages
                .Send(gmailMessage, "me")
                .ExecuteAsync();

            _logger.LogInformation(
                "[EmailService] Correo de primer login enviado por GmailApi a {Email}.",
                toEmail);
        }

        private static UserCredential CreateUserCredential(
            string clientId,
            string clientSecret,
            string refreshToken)
        {
            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            var token = new TokenResponse
            {
                RefreshToken = refreshToken
            };

            var flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = new[] { GmailService.Scope.GmailSend }
                });

            return new UserCredential(flow, "me", token);
        }

        private static string ConvertMimeMessageToBase64Url(MimeMessage mimeMessage)
        {
            using var stream = new MemoryStream();

            mimeMessage.WriteTo(stream);

            return Convert.ToBase64String(stream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
