using Unstapp.Application.DTOs;
using Unstapp.Application.DTOs.Posts;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IPostService
    {
        Task<ServiceResult<List<PostDto>>> GetPostsByUserAsync(int userId);
        Task<ServiceResult<PostDto>> CreateAsync(int userId, CreatePostDto dto);
        Task<ServiceResult<PaginatedPostsResponseDto>> GetAllAsync(int userId, PostFilter filter, int page, int limit);
        Task<ServiceResult<PostDto>> GetByIdAsync(int userId, int postId);
        Task<ServiceResult<bool>> DeleteAsync(int postId, int currentUserId);
    }
}
