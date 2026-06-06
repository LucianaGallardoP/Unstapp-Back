using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<List<CalendarEvent>> GetEventsByRangeAsync(DateTime start, DateTime end)
        {
            return await _context.CalendarEvents
                .AsNoTracking()
                .Where(e =>
                    !e.IsDeleted &&
                    e.StartDate <= end &&
                    e.EndDate >= start)
                .OrderBy(e => e.StartDate)
                .ToListAsync();
        }

        public async Task AddAsync(CalendarEvent calendarEvent)
        {
            await _context.CalendarEvents.AddAsync(calendarEvent);
            await _context.SaveChangesAsync();
        }
    }
}
