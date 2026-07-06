using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Unstapp.Shared.DTOs.Moderation;
using Unstapp.Shared.Interfaces;

namespace Unstapp.Infrastructure.Services
{
    public class GeminiModerationService : IModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<GeminiModerationService> _logger;

        public GeminiModerationService(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<GeminiModerationService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<ModerationResultDto> ModeratePostAsync(string? content)
        {
            if(string.IsNullOrWhiteSpace(content))
                return new ModerationResultDto
                {
                    IsApproved = true,
                    Code = "APPROVED",
                    Message = "Contenido aprobado."
                };

            var enabled = _config.GetValue<bool>("GeminiModeration:Enabled");

            if(!enabled)
                return new ModerationResultDto
                {
                    IsApproved = true,
                    Code = "MODERATION_DISABLED",
                    Message = "Moderación Desactivada."
                };

            var apiKey = _config["GeminiModeration:ApiKey"];
            var model = _config["GeminiModeration:Model"] ?? "gemini-1.5-flash";
            var blockOnServiceError = _config.GetValue<bool>("GeminiModeration:BlockOnServiceError");

            if(string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Gemini API key no configurada.");
                return blockOnServiceError
                    ? RejectByServiceError()
                    : ApproveByFallback();
            }

            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var prompt = $@"
                    Actuá como un sistema de moderación de contenido para una red social universitaria.

                    Analizá el siguiente texto y decidí si puede publicarse.

                    Debe rechazarse si contiene:
                    - insultos
                    - lenguaje discriminatorio
                    - amenazas
                    - acoso
                    - violencia explícita
                    - contenido sexual explícito
                    - incitación al odio
                    - lenguaje muy ofensivo contra una persona o grupo
                    - spam evidente

                    Debe aprobarse si es una publicación normal, académica, informativa o social.

                    Respondé únicamente en JSON válido, sin markdown, con este formato exacto:

                    {{
                      ""approved"": true,
                      ""category"": ""safe"",
                      ""reason"": ""Contenido permitido.""
                    }}

                    o

                    {{
                      ""approved"": false,
                      ""category"": ""toxic_language"",
                      ""reason"": ""Motivo breve del rechazo.""
                    }}

                    Texto a analizar:
                    ""{content}""
                    ";
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0,
                    responseMimeType = "application/json"
                },
                safetySettings = new[]
                {
                    new
                    {
                        category = "HARM_CATEGORY_HATE_SPEECH",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_HARASSMENT",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    },
                    new
                    {
                        category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                        threshold = "BLOCK_MEDIUM_AND_ABOVE"
                    }
                }
            };

            try
            {
                HttpResponseMessage? response = null;
                for(var attemp = 1; attemp <= 3; attemp++)
                {
                    response = await _httpClient.PostAsJsonAsync(endpoint, requestBody);

                    if(response.IsSuccessStatusCode)
                        break;

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        var delay = TimeSpan.FromMilliseconds(500 * attemp);
                        await Task.Delay(delay);
                        continue;
                    }

                    break;
                }

                if(response == null || !response.IsSuccessStatusCode)
                {

                    var errorContent = response == null
                        ? "Sin respuesta de Gemini API."
                        : await response.Content.ReadAsStringAsync();


                    _logger.LogWarning(
                        "Error en Gemini API. StatusCode: {StatusCode}, Response: {Response}",
                        response?.StatusCode,
                        errorContent
                    );

                    return blockOnServiceError
                        ? RejectByServiceError()
                        : ApproveByFallback();
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                return ParseGeminiResponse(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al moderar el contenido con Gemini");

                return blockOnServiceError
                    ? RejectByServiceError()
                    : ApproveByFallback();
            }
        }

        private static ModerationResultDto ParseGeminiResponse(string responseContent)
        {
            using var document = JsonDocument.Parse(responseContent);

            var root = document.RootElement;

            if(!root.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
                return new ModerationResultDto
                {
                    IsApproved = false,
                    Code = "INVALID_RESPONSE",
                    Message = "Respuesta inválida de Gemini."
                };

            var candidate = candidates[0];

            if(!candidate.TryGetProperty("content", out var contentElement) ||
                !contentElement.TryGetProperty("parts", out var parts) ||
                parts.GetArrayLength() == 0)
                return new ModerationResultDto
                {
                    IsApproved = false,
                    Code = "INVALID_RESPONSE",
                    Message = "Respuesta inválida de Gemini."
                };

            var text = parts[0].GetProperty("text").GetString();

            if(string.IsNullOrWhiteSpace(text))
                return new ModerationResultDto
                {
                    IsApproved = false,
                    Code = "INVALID_RESPONSE",
                    Message = "Respuesta inválida de Gemini."
                };

            text = CleanJsonResponse(text);

            using var moderationDocument = JsonDocument.Parse(text);

            var moderationRoot = moderationDocument.RootElement;

            var approved = moderationRoot.GetProperty("approved").GetBoolean();

            var category = moderationRoot.TryGetProperty("category", out var categoryElement)
                ? categoryElement.GetString()
                : null; 

            var reason = moderationRoot.TryGetProperty("reason", out var reasonElement)
                ? reasonElement.GetString()
                : null;

            if(!approved)
                return new ModerationResultDto
                {
                    IsApproved = false,
                    Code = "POST_CONTENT_VIOLATES_COMMUNITY_RULES",
                    Message = "Tu publicación contiene lenguaje que infringe las normas de la comunidad.",
                    Category = category
                };

            return new ModerationResultDto
            {
                IsApproved = true,
                Code = "APPROVED",
                Message = reason ?? "Contenido aprobado.",
                Category = category
            };
        }

        private static string CleanJsonResponse(string text)
        {
            return text
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Trim();
        }

        private static ModerationResultDto ApproveByFallback()
        {
            return new ModerationResultDto
            {
                IsApproved = true,
                Code = "MODERATION_SERVICE_UNAVAILABLE_APPROVED",
                Message = "Moderación no disponible, contenido aprobado por fallback."
            };
        }

        private static ModerationResultDto RejectByServiceError()
        {
            return new ModerationResultDto
            {
                IsApproved = false,
                Code = "MODERATION_SERVICE_UNAVAILABLE",
                Message = "Error en el servicio de moderación, contenido rechazado por seguridad.",
                Category = "service_error"
            };
        }
    }
}
