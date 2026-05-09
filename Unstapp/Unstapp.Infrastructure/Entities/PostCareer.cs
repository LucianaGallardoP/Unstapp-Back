using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Infrastructure.Entities
{
    public class PostCareer
    {
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int CareerId { get; set; }
        public Career Career { get; set; } = null!;
    }
}
