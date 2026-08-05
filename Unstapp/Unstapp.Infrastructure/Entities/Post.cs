using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.Infrastructure.Entities
{
    public class Post
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string? Content { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime PostDate { get; set; } = DateTime.UtcNow;
        public PostCategory Category { get; set; } = PostCategory.General;
        public bool IsImportant { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public User User { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<PostCareer> PostCareers { get; set; } = new List<PostCareer>();
    }
}
