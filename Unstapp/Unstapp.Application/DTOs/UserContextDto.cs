using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class UserContextDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public List<UserContextCareerDto> CareerDtos { get; set; } = new();
    }
}
