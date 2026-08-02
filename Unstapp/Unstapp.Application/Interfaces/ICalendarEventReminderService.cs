using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface ICalendarEventReminderService
    {
        Task<ServiceResult<bool>> UpdateReminderAsync(int userId, int calendarEventId, bool enabled);
        Task ProcessDueRemindersAsync(CancellationToken cancellationToken = default);
    }
}
