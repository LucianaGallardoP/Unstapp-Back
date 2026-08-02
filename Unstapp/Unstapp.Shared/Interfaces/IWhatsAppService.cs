using Unstapp.Shared.DTOs.Whatsapp;
using Unstapp.Shared.DTOs.WhatsApp;

namespace Unstapp.Shared.Interfaces
{
    public interface IWhatsAppService
    {
        Task SendImportantPostTemplateAsync(WhatsAppTemplateMessageDto dto);
        Task<bool> SendCalendarEventReminderTemplateAsync(CalendarEventReminderWhatsAppDto dto);
    }
}
