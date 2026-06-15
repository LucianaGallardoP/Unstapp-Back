using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class CalendarTypeCountsDto
    {
        public int Examenes { get; set; }
        public int Clases { get; set; }
        public int Eventos { get; set; }
        public int Feriados { get; set; }
    }
}
