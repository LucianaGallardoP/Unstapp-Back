using Microsoft.EntityFrameworkCore;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.Helpers;

namespace Unstapp.Infrastructure.Repositories
{
    public class CalendarEventReminderRepository : ICalendarEventReminderRepository
    {
        private readonly AppDbContext _context;

        public CalendarEventReminderRepository(AppDbContext context )
        {
            _context = context;
        }

        public async Task<CalendarEventReminder?> GetReminderAsync(int userId, int calendarEventId)
        {
            return await _context.CalendarEventReminders
                .FirstOrDefaultAsync(
                    r => r.UserId == userId &&
                    r.CalendarEventId == calendarEventId
                );
        }

        public async Task AddAsync(CalendarEventReminder reminder)
        {
            await  _context.CalendarEventReminders.AddAsync(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CalendarEventReminder reminder)
        {
            _context.CalendarEventReminders.Update(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CalendarEventReminder>> GetDueRemindersAsync(DateOnly maxEventDate)
        {
            var argentinaTimeZone = DateHelper.GetArgentinaTimeZone();

            var utcNow = DateTime.UtcNow;

            var endArgentina = DateTime.SpecifyKind(
                maxEventDate.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Unspecified
            );

            var endUtc = DateHelper.ConvertArgentinaLocalToUtc(endArgentina);

            return await _context.CalendarEventReminders
                .Include(r => r.User)
                .Include(r => r.CalendarEvent)
                .Where(r =>
                    r.IsEnabled &&
                    r.SentAt == null &&
                    !r.CalendarEvent.IsDeleted &&
                    r.CalendarEvent.StartDate >= utcNow &&
                    r.CalendarEvent.StartDate < endUtc
                )
                .ToListAsync();
        }

        public async Task MarkAsSentAsync(
            CalendarEventReminder reminder,
            bool appNotificationSent,
            bool whatsAppSent)
        {
            reminder.SentAt = DateTime.UtcNow;
            reminder.AppNotificationSent = appNotificationSent;
            reminder.WhatsAppSent = whatsAppSent;

            _context.CalendarEventReminders.Update(reminder);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetEnabledReminderEventIdsAsync(int userId, List<int> calendarEventIds)
        {
            if (calendarEventIds.Count == 0)
                return new List<int>();

            return await _context.CalendarEventReminders
                .AsNoTracking()
                .Where(r =>
                    r.UserId == userId &&
                    r.IsEnabled &&
                    calendarEventIds.Contains(r.CalendarEventId))
                .Select(r => r.CalendarEventId)
                .ToListAsync();
        }
    }
}
