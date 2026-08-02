using Microsoft.EntityFrameworkCore;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Infrastructure.Repositories
{
    public class CalendarEventRepository : ICalendarEventRepository
    {
        private readonly AppDbContext _context;

        public CalendarEventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CalendarEvent>> GetEventsByRangeAsync(
            DateTime start,
            DateTime end,
            List<int> userCareerIds,
            bool includeAllCareers)
        {
            var query = _context.CalendarEvents
                .AsNoTracking()
                .Include(e => e.CalendarEventCareers)
                    .ThenInclude(ec => ec.Career)
                .Where(e => !e.IsDeleted &&
                    e.StartDate >= start &&
                    e.StartDate < end
                );

            if(!includeAllCareers)
            {
                query = query.Where(e =>
                    !e.CalendarEventCareers.Any() ||
                    e.CalendarEventCareers.Any(ec => userCareerIds.Contains(ec.CareerId))
                );
            }

            return await query
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task AddAsync(CalendarEvent calendarEvent)
        {
            await _context.CalendarEvents.AddAsync(calendarEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<CalendarEvent?> GetByIdAsync(int eventId)
        {
            return await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.CalendarEventId == eventId);
        }

        public async Task DeleteEventAsync(CalendarEvent calendarEvent)
        {
            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();
        }

        public async Task AddWithCareerAsync(CalendarEvent calendarEvent, List<int> careerIds)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.CalendarEvents.AddAsync(calendarEvent);
                await _context.SaveChangesAsync();

                if (careerIds.Count > 0)
                {
                    var eventCareers = careerIds
                        .Distinct()
                        .Select(careerId => new CalendarEventCareer
                        {
                            CalendarEventId = calendarEvent.CalendarEventId,
                            CareerId = careerId
                        })
                        .ToList();

                    await _context.CalendarEventCareers.AddRangeAsync(eventCareers);
                    await _context.SaveChangesAsync();

                    calendarEvent.CalendarEventCareers = eventCareers;
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
