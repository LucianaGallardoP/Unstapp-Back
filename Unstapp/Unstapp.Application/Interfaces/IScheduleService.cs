using System.Collections.Generic;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IScheduleService
    {
        Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByDayAsync(int userId, string day);
        Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByCareerAsync(int careerId, string day);

        Task<ServiceResult<ScheduleResponseDto>> CreateScheduleAsync(ScheduleCreateDto dto);
    }
}