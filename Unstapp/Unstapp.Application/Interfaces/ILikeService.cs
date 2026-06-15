using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;

namespace Unstapp.Application.Interfaces
{
    public interface ILikeService
    {
        Task<ToggleLikeResult> ToggleLikeAsync(int postId, int userId);
    }
}
