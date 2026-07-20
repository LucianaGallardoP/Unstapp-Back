using Unstapp.Application.DTOs;

namespace Unstapp.Application.Interfaces
{
    public interface INotificationRealtimeSender
    {
        Task SendNotificationAsync(int recipientUserId, NotificationResponseDto notification);
    }
}
