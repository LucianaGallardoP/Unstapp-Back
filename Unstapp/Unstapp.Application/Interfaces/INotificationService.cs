using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateLikeNotificationAsync(int actorUserId, int postId);
        Task CreateCommentNotificationAsync(int actorUserId, int postId);
        Task CreateFollowNotificationAsync(int actorUserId, int followedUserId);
        Task<ServiceResult<List<NotificationResponseDto>>> GetMyNotificationsAsync(int userId);
        Task<ServiceResult<bool>> MarkAsReadAsync(int userId, int notificationId);
        Task<ServiceResult<bool>> DeleteAsync(int userId, int notificationId);
        Task<ServiceResult<bool>> MarkAllAsReadAsync(int userId);
        Task<ServiceResult<bool>> DeleteAllAsync(int userId);
        Task<ServiceResult<bool>> HasUnreadNotificationsAsync(int userId);
    }
}
