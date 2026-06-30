using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs.Horarios
{
    public class ScheduleUpdateDto
    {
        public int? CareerId { get; set; }
        public string? Subject { get; set; }
        public string? Professor { get; set; }
        public string? Classroom {  get; set; }
        public string? Day { get; set; }
        public string? StartTime { get; set; }
        public decimal? DurationHours { get; set; }
    }
}
