namespace Unstapp.Infrastructure.Entities
{
    public class User
    {
        public int UserId {  get; set; }
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string DNI { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Password { get; set; }
        public string? Bio { get; set; }
        public bool FirstTime { get; set; } = true;
        public string? Biography { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<UserCareer> UserCareers { get; set; } = new List<UserCareer>();
        public ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
        public ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
        public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
        public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>();
    }
}
