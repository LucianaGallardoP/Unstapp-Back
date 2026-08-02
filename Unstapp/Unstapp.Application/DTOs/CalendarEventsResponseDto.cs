using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs.Calendar;

namespace Unstapp.Application.DTOs
{
    public class CalendarEventsResponseDto
    {
        public List<CalendarEventDto> Events { get; set; } = new();
        public CalendarTypeCountsDto TypeCounts { get; set; } = new();
        public List<DateOnly> EventDays { get; set; } = new();
    }
}
