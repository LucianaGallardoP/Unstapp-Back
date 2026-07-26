namespace Unstapp.Application.DTOs.Posts
{
    public class PaginatedPostsResponseDto
    {
        public List<PostDto> Posts { get; set; }
        public bool HasMore { get; set; }
    }
}
