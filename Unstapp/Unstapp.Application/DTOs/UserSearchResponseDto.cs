using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class UserSearchResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; } 
    }
}
