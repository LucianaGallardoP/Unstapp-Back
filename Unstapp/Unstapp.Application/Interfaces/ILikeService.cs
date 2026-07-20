using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface ILikeService
    {
        Task<ServiceResult<bool>> ToggleLikeAsync(int postId, int userId);
    }
}
