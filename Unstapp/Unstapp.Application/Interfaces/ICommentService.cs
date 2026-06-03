using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentResponseDto>?> GetAllByPostAsync(int postId);
        Task<CommentResponseDto?> AddAsync(int postId, int userId, CreateCommentDto dto);
        Task<ServiceResult<bool>> DeleteAsync(int commentId, int currentUserId);
    }
}
