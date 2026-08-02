using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetAllByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Include(n => n.ActorUser)
                .Include(n => n.CalendarEvent)
                .Where(n => n.RecipientUserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }

        public async Task MarkAllAsReadByUserIdAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsDeleted && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAllByUserIdAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsDeleted)
                .ToListAsync();

            foreach(var notification in notifications)
            {
                notification.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUnreadByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .AnyAsync(n =>
                n.RecipientUserId == userId &&
                !n.IsRead &&
                !n.IsDeleted);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
