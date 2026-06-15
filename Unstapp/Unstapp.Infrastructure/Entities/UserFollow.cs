using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Infrastructure.Entities
{
    public class UserFollow
    {
        public int FollowerUserId { get; set; }
        public User FollowerUser { get; set; } = null!;

        public int FollowedUserId { get; set; }
        public User FollowedUser { get; set; } = null!;

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
