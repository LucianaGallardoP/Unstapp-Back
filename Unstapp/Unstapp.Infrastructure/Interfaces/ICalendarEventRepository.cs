using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface ICalendarEventRepository
    {
        Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end);
        Task AddAsync(CalendarEvent calendarEvent);
        Task<CalendarEvent?> GetByIdAsync(int eventId);
        Task DeleteEventAsync(CalendarEvent calendarEvent);
    }
}
