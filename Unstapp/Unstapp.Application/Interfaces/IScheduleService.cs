using Unstapp.Application.DTOs;
using Unstapp.Application.DTOs.Horarios;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IScheduleService
    {
        Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByDayAsync(int userId, string day);
        Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByCareerAsync(int careerId, string day);
        Task<ServiceResult<ScheduleResponseDto>> CreateScheduleAsync(ScheduleCreateDto dto);
        Task<ServiceResult<ScheduleResponseDto>> UpdateScheduleAsync(int id, ScheduleUpdateDto dto);
        Task<ServiceResult<bool>> DeleteScheduleAsync(int id);
    }
}