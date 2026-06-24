using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Infrastructure.Interfaces
{
    public interface IScheduleRepository
    {
        Task<List<Schedule?>> GetSchedulesByCareerAndDayAsync(int careerId, string day);
        Task AddNewScheduleAsync(Schedule schedule);
        Task<Schedule> GetScheduleByIdAsync(int scheduleId);
    }
}
