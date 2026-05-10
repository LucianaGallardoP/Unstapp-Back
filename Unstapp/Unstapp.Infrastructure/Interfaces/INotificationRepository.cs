using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    internal interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetAllByUserIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int notificationId);
        Task SaveChangesAsync();
    }
}
