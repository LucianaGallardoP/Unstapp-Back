using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Unstapp.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
