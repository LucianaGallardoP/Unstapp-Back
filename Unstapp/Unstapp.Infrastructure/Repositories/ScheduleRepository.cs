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
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly AppDbContext _context;

        public ScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Schedule?>> GetSchedulesByCareerAndDayAsync(int careerId, string day)
        {
            return await _context.Schedules
                .Where(s => s.CareerId == careerId && s.Day.ToLower() == day.ToLower())
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task AddNewScheduleAsync(Schedule schedule)
        {
            await _context.AddAsync(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<Schedule> GetScheduleByIdAsync(int scheduleId)
        {
            return await _context.Schedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId);
        }
    }
}
