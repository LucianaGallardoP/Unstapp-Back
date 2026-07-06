using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs.Moderation
{
    public class ModerationResultDto
    {
        public bool IsApproved { get; set; }
        public string Code { get; set; } = "OK";
        public string Message { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}
