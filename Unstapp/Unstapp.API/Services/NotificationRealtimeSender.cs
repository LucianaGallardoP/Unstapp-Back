using Microsoft.AspNetCore.SignalR;
using Unstapp.API.Hubs;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;

namespace Unstapp.API.Services
{
    public class NotificationRealtimeSender : INotificationRealtimeSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationRealtimeSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(int recipientUserId, NotificationResponseDto notification)
        {
            await _hubContext.Clients.User(recipientUserId.ToString()).SendAsync("ReceiveNotification", notification);
        }
    }
}
