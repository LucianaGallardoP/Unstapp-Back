using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.Infrastructure.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }

        public int RecipientUserId { get; set; }
        public User RecipientUser { get; set; } = null!;

        public int ActorUserId { get; set; }
        public User ActorUser { get; set; } = null!;

        public string ActorUserName = string.Empty;

        public NotificationActionType ActionType { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public bool IsPriority { get; set; } = false;

        public bool IsRead { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
