using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface ICommentRepository
    {
        public Task<List<Comment>> GetAllByPostWithRelationsAsync(int postId);
        Task AddAsync(Comment comment);
        Task<Comment?> GetByIdWithRelationsAsync(int commentId);
        Task DeleteAsync(Comment comment);
    }
}
