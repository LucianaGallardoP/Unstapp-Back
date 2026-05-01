using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Infrastructure.Entities
{
    public class Post
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int? SubjectId { get; set; }
        public string Content { get; set; } = null!;
        public string? MediaUrl { get; set; }
        public DateTime PostDate { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
