using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface ICalendarService
    {
        Task<ServiceResult<CalendarEventsResponseDto>> GetEventsByRangeAsync(
            DateTime start,
            DateTime end);
        Task<ServiceResult<CalendarEventDto>> CreateEventAsync(
            CreateCalendarEventDto dto,
            int currentUserId);
        Task<ServiceResult<List<CalendarEventDto>>> GetTodayEventsAsync();
        Task<ServiceResult<List<CalendarEventDto>>> GetEventsByDayAsync(DateTime date);
        Task<ServiceResult<bool>> DeleteEventAsync(int eventId);
    }
}
