using System;

namespace Unstapp.Infrastructure.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public int CareerId { get; set; }
        public string Day { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public decimal DurationHours { get; set; }
        public string Professor { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;

        // Propiedad de navegación virtual para EF Core
        public virtual Career? Career { get; set; }
    }
}