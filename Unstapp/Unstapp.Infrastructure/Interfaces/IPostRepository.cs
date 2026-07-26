using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<Post?> GetByIdWithRelationsAsync(int postId);
        Task<List<Post>> GetAllWithRelationsAsync();
        Task<List<Post>> GetPostsByUserAsync(int userId);
        Task<bool> PostExistsAsync(int postId);
        Task<(List<Post> Posts, bool HasMore)> GetFilteredPostsAsync(int userId, PostFilter filter, int page, int limit);
        Task<List<Post>> SearchPostsAsync(string term);
        Task<Post?> GetByIdIncludingDeletedAsync(int postId);
        Task SoftDeleteAsync(Post post);
        Task AddPostWithCareersAsync(Post post, List<int> careerIds);
    }
}