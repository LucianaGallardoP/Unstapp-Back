using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateLikeNotificationAsync(int actorUserId, int postId);
        Task CreateCommentNotificationAsync(int actorUserId, int postId);
        Task<ServiceResult<List<NotificationResponseDto>>> GetMyNotificationsAsync(int userId);
        Task<ServiceResult<bool>> MakeAsReadAsync(int userId, int notificationId);
        Task<ServiceResult<bool>> DeleteAsync(int userId, int notificationId);
    }
}
