namespace Unstapp.Infrastructure.Entities
{
    public class CalendarEventCareer
    {
        public int CalendarEventId { get; set; }
        public int CareerId { get; set; }

        public CalendarEvent CalendarEvent { get; set; } = null!;
        public Career Career { get; set; } = null!;
    }
}
