using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
