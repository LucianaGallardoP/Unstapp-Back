using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Infrastructure.Entities
{
    public class UserCareer
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int CareerId { get; set; }
        public Career Career { get; set; } = null!;
    }
}
