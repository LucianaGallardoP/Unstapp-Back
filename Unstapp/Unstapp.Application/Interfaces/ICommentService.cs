using Unstapp.Application.DTOs.Comentarios;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface ICommentService
    {
        Task<ServiceResult<List<CommentResponseDto>?>> GetAllByPostAsync(int postId);
        Task<ServiceResult<CommentResponseDto>> AddAsync(int postId, int userId, CreateCommentDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int commentId, int currentUserId);
    }
}
