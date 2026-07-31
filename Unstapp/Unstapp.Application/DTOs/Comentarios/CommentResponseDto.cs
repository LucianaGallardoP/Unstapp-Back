namespace Unstapp.Application.DTOs.Comentarios
{
    public class CommentResponseDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }
}
