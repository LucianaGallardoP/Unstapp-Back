using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public string User {  get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int PostId { get; set; }
        public bool IsPriority { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
