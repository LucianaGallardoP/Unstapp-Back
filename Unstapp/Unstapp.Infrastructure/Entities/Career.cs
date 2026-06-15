using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Infrastructure.Entities
{
    public class Career
    {
        public int CareerId { get; set; }
        public string Name { get; set; } = string.Empty;

        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; } = null!;

        public ICollection<UserCareer> UserCareers { get; set; } = new List<UserCareer>();
        public ICollection<PostCareer> PostCareers { get; set; } = new List<PostCareer>();
    }
}
