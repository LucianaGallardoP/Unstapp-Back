using Microsoft.Extensions.Logging;
using Unstapp.Infrastructure.DTOs.WhatsApp;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Whatsapp;
using Unstapp.Shared.Interfaces;

namespace Unstapp.Infrastructure.Services
{
    public class WhatsappNotificationService : IWhatsAppNotificationService
    {
        private readonly IWhatsAppNotificationRepository _whatsAppNotificationRepository;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<WhatsappNotificationService> _logger;

        public WhatsappNotificationService(
            IWhatsAppNotificationRepository whatsAppNotificationRepository,
            IWhatsAppService whatsAppService,
            ILogger<WhatsappNotificationService> logger
        )
        {
            _whatsAppNotificationRepository = whatsAppNotificationRepository;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        public async Task NotifyImportantPostAsync(int postId)
        {
            var post = await _whatsAppNotificationRepository.GetImportantAdministrationPostAsync(postId);

            if (post == null)
            {
                _logger.LogInformation(
                    "El post {PostId} no requiere notificación por WhatsApp.",
                    postId
                );

                return;
            }

            List<WhatsAppRecipientDto> recipients;

            if(post.CareerIds.Count == 0)
            {
                _logger.LogInformation(
                    "El post {PostId} no tiene carreras asociadas. Se enviará a todos los alumnos con WhatsApp activo.",
                    postId
                );

                recipients = await _whatsAppNotificationRepository.GetStudentsWithWhatsAppEnabledAsync();
            }
            else
            {
                recipients = await _whatsAppNotificationRepository.GetStudentsWithWhatsAppEnabledByCareerIdsAsync(post.CareerIds);
            }

            if(recipients.Count == 0)
            {
                _logger.LogInformation(
                    "No hay alumnos con Whatsapp activo para el post {PostId}.",
                    postId
                );

                return;
            }
            var destinationText = GetDestinationText(post);
            foreach (var recipient in recipients)
            {
                await _whatsAppService.SendImportantPostTemplateAsync(new WhatsAppTemplateMessageDto
                {
                    ToPhoneNumber = recipient.PhoneNumber,
                    StudentName = recipient.FullName,
                    PostTitle = post.Content,
                    Subject =  destinationText,
                    DateText = post.PostDate.ToString("dd/MM/yyyy")
                });
            }

            _logger.LogInformation(
                "Se procesaron {Count} notificaciones de WhatsApp para el post {PostId}.",
                recipients.Count,
                postId
            );
        }

        private static string GetDestinationText(ImportantPostWhatsAppDto post)
        {
            if (post.CareerIds.Count == 0)
                return "Todas las carreras.";

            if (post.CareerNames.Count == 0)
                return "Carreras seleccionadas.";

            var text = string.Join(", ", post.CareerNames);

            if(text.Length <= 250)
                return text;

            return $"{post.CareerNames.Count} carreras seleccionadas.";
        }
    }
}
