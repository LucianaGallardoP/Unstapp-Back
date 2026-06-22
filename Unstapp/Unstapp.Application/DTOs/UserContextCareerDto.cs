using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class UserContextCareerDto
    {
        public int CareerId { get; set; }
        public string CareerName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
    }
}
