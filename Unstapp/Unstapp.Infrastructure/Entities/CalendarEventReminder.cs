using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Infrastructure.Entities
{
    public class CalendarEventReminder
    {
        public int CalendarEventReminderId { get; set; }

        public int CalendarEventId { get; set; }
        public CalendarEvent CalendarEvent { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public bool AppNotificationSent { get; set; }
        public bool WhatsAppSent { get; set; }
    }
}
