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
        public string Password { get; set; } = null!;
        public byte[]? Avatar { get; set; }
        public string? Bio { get; set; }
        public bool FirstTime { get; set; } = true;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
