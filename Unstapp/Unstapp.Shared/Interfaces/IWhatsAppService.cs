using Unstapp.Shared.DTOs.Whatsapp;

namespace Unstapp.Shared.Interfaces
{
    public interface IWhatsAppService
    {
        Task SendImportantPostTemplateAsync(WhatsAppTemplateMessageDto dto);
    }
}
