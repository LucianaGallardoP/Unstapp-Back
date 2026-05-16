using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Application.DTOs
{
    public class ProfileResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public List<string> Careers { get; set; } = new();
        public string? Bio {  get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public bool IsOwnProfile { get; set; }
        public bool IsFollowing { get; set; }
    }
}
