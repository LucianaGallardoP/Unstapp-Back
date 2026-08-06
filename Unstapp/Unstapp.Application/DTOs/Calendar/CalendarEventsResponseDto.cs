namespace Unstapp.Application.DTOs.Calendar
{
    public class CalendarEventsResponseDto
    {
        public List<CalendarEventDto> Events { get; set; } = new();
        public CalendarTypeCountsDto TypeCounts { get; set; } = new();
        public List<DateOnly> EventDays { get; set; } = new();
    }
}
