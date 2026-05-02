using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class PostDto
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? MediaUrl { get; set; }
        public DateTime PostDate { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool isLikedByMe { get; set; } = false;
    }
}
