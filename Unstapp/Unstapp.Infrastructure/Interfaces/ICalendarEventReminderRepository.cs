using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface ICalendarEventReminderRepository
    {
        Task<CalendarEventReminder?> GetReminderAsync(int userId, int calendarEventId);
        Task AddAsync(CalendarEventReminder reminder);
        Task UpdateAsync(CalendarEventReminder reminder);
        Task<List<CalendarEventReminder>> GetDueRemindersAsync(DateOnly eventDate);
        Task MarkAsSentAsync(CalendarEventReminder reminder, bool appNotificationSent, bool WhatsAppSent);
        Task<List<int>> GetEnabledReminderEventIdsAsync(int userId, List<int> calendarEventIds);
    }
}
