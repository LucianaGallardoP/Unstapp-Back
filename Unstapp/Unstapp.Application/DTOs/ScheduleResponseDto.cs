using System;

namespace Unstapp.Application.DTOs
{
    public class ScheduleResponseDto
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public decimal DurationHours { get; set; }
        public string Professor { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;
    }
}