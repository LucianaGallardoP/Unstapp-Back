using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class ScheduleImportResponseDto
    {
        public int CreatedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
