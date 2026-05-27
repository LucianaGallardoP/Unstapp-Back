using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Infrastructure.Entities
{
    public class Faculty
    {
        public int FacultyId { get; set; }
        public string Name = string.Empty;
        public ICollection<Career> Careers { get; set; } = new List<Career>();
    }
}
