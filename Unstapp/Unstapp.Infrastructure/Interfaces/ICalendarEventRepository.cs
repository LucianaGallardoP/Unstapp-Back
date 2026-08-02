using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface ICalendarEventRepository
    {
        Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end, List<int> userCareerIds, bool includeAllCareers);
        Task AddAsync(CalendarEvent calendarEvent);
        Task<CalendarEvent?> GetByIdAsync(int eventId);
        Task DeleteEventAsync(CalendarEvent calendarEvent);
        Task AddWithCareerAsync(CalendarEvent calendarEvent, List<int> careerIds);
    }
}
