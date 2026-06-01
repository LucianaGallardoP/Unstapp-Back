using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<Post?> GetByIdWithRelationsAsync(int postId);
        Task<List<Post>> GetAllWithRelationsAsync();
        Task<List<Post>> GetPostsByUserAsync(int userId);
        Task<bool> PostExistsAsync(int postId);
        Task<List<Post>> GetFilteredPostsAsync(int userId, PostFilter filter);
        Task<List<Post>> SearchPostsAsync(string term);
        Task<Post?> GetByIdIncludingDeletedAsync(int postId);
        Task SoftDeleteAsync(Post post);
    }
}