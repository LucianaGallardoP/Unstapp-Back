using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Unstapp.Shared.DTOs.Whatsapp;
using Unstapp.Shared.DTOs.WhatsApp;
using Unstapp.Shared.Interfaces;

namespace Unstapp.Infrastructure.Services
{
    public class WhatsappService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<WhatsappService> _logger;

        public WhatsappService(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<WhatsappService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task SendImportantPostTemplateAsync(WhatsAppTemplateMessageDto dto)
        {
            var enabled = bool.TryParse(_config["WhatsApp:Enabled"], out var parsedEnabled) && parsedEnabled;

            if (!enabled)
            {
                _logger.LogInformation("WhatsApp desactivado. No se envió notificación.");
                return;
            }

            var apiVersion = _config["WhatsApp:ApiVersion"] ?? "v22.0";
            var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
            var accessToken = _config["WhatsApp:AccessToken"];
            var templateName = _config["WhatsApp:ImportantPostTemplateName"];
            var languageCode = _config["WhatsApp:LanguageCode"] ?? "es_AR";

            if (string.IsNullOrWhiteSpace(phoneNumberId) ||
                string.IsNullOrWhiteSpace(accessToken) ||
                string.IsNullOrWhiteSpace(templateName))
            {
                _logger.LogError("Faltan configuraciones de WhatsApp. No se envió notificación.");
                return;
            }

            var endpoint = $"https://graph.facebook.com/{apiVersion}/{phoneNumberId}/messages";

            var normalizedPhone = NormalizePhoneNumber(dto.ToPhoneNumber);

            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                _logger.LogWarning("Teléfono inválido para WhatsApp: {PhoneNumber}. No se envió notificación.", dto.ToPhoneNumber);
                return;
            }

            var body = new
            {
                messaging_product = "whatsapp",
                to = normalizedPhone,
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new
                    {
                        code = languageCode
                    },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new object[]
                            {
                                new { type = "text", text = SanitizeTemplateText(dto.StudentName) },
                                new { type = "text", text = SanitizeTemplateText(dto.PostTitle) },
                                new { type = "text", text = SanitizeTemplateText(dto.SenderName) },
                                new { type = "text", text = SanitizeTemplateText(dto.Subject) },
                                new { type = "text", text = SanitizeTemplateText(dto.DateText) }
                        }
                    }
                }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = JsonContent.Create(body);

            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if(!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Error al enviar notificación de WhatsApp al número {Phone}. Código de estado: {StatusCode}. Respuesta: {ResponseText}",
                    normalizedPhone,
                    response.StatusCode,
                    responseText
                );
                return;
            }

            _logger.LogInformation(
                "Notificación de WhatsApp enviada al número {Phone}. Respuesta: {ResponseText}",
                normalizedPhone,
                responseText
            );
        }

        public async Task<bool> SendCalendarEventReminderTemplateAsync(CalendarEventReminderWhatsAppDto dto)
        {
            var enabled = _config.GetValue<bool>("WhatsApp:Enabled");

            if (!enabled)
                return false;

            var phoneNumberId = _config["WhatsApp:PhoneNumberId"];
            var accessToken = _config["WhatsApp:AccessToken"];
            var apiVersion = _config["WhatsApp:ApiVersion"] ?? "v25.0";

            var templateName = _config["WhatsApp:CalendarEventReminderTemplateName"]
                ?? "unstapp_recordatorio_evento";

            var languageCode = _config["WhatsApp:LanguageCode"] ?? "es_AR";

            if (string.IsNullOrWhiteSpace(phoneNumberId) || string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogError("No se pudo enviar WhatsApp: falta PhoneNumberId o AccessToken.");

                return false;
            }

            var url = $"https://graph.facebook.com/{apiVersion}/{phoneNumberId}/messages";

            var body = new
            {
                messaging_product = "whatsapp",
                to = NormalizePhoneNumber(dto.ToPhoneNumber),
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new
                    {
                        code = languageCode
                    },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new object[]
                            {
                                new { type = "text", text = SanitizeTemplateText(dto.StudentName) },
                                new { type = "text", text = SanitizeTemplateText(dto.EventTitle) },
                                new { type = "text", text = SanitizeTemplateText(dto.EventType) },
                                new { type = "text", text = SanitizeTemplateText(dto.EventDate) },
                                new { type = "text", text = SanitizeTemplateText(dto.EventTime) },
                                new { type = "text", text = SanitizeTemplateText(dto.Description) }
                            }
                        }
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            request.Content = JsonContent.Create(body);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                _logger.LogError("Error enviando recordatorio de evento por WhatsApp. Status: {Status}. Error: {Error}", response.StatusCode, error);

                return false;
            }

            return true;
        }
        private static string NormalizePhoneNumber(string phone)
        {
            if(string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            var cleaned = phone
                .Replace("+", "")
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();

            return cleaned;
        }

        private static string SanitizeTemplateText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "-";

            return value.Trim();
        }
    }
}
