using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class CreatePostDto
    {
        public int UserId { get; set; }
        public int? SubjectId { get; set; }
        public string Content { get; set; } = null!;
    }
}
