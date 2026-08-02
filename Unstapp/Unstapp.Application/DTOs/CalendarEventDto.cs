namespace Unstapp.Application.DTOs
{
    public class CalendarEventDto
    {
        public int CalendarEventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Day {  get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool ReminderEnabledForCurrentUser { get; set; }
    }
}