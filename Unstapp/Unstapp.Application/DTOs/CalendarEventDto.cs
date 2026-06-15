using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.Application.DTOs
{
    public class CalendarEventDto
    {
        public int CalendarEventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CalendarEventType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
