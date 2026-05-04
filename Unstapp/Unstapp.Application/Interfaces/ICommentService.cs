using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;

namespace Unstapp.Application.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentResponseDto>?> GetAllByPostAsync(int postId);
    }
}
