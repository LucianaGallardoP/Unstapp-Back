using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs.Horarios;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Interfaces
{
    public interface IScheduleService
    {
        Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByDayAsync(int userId, string day, int year);
        Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByCareerAsync(int careerId, string day, int year);
        Task<ServiceResult<ScheduleResponseDto>> CreateScheduleAsync(ScheduleCreateDto dto);
        Task<ServiceResult<ScheduleResponseDto>> UpdateScheduleAsync(int id, ScheduleUpdateDto dto);
        Task<ServiceResult<bool>> DeleteScheduleAsync(int id);
        Task<ServiceResult<ScheduleImportResponseDto>> ImportSchedulesAsync(IFormFile file);
    }
}