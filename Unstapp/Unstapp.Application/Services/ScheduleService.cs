using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ICareerRepository _careerRepository;
        private readonly IScheduleRepository _scheduleRepository;

        public ScheduleService(
            ICareerRepository careerRepository,
            IScheduleRepository scheduleRepository
        )
        {
            _careerRepository = careerRepository;
            _scheduleRepository = scheduleRepository;
        }


        public async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByDayAsync(int userId, string day)
        {

            var userCareer = await _careerRepository.GetUserCareerAsync(userId);
            if (userCareer == null)
            {
                return ServiceResult<List<ScheduleResponseDto>>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_ASSIGNED",
                    "El usuario no tiene una carrera asignada."
                );
            }

            if (userCareer.Career == null)
            {
                return ServiceResult<List<ScheduleResponseDto>>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_FOUND",
                    "La carrera asignada al usuario no fue encontrada."
                );
            }
            var careerId = userCareer.CareerId;
            return await GetSchedulesCoreAsync(careerId, day);
        }


        public async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByCareerAsync(int careerId, string day)
        {

            var careerExists = await _careerRepository.CareerExistsAsync(careerId);
            if (!careerExists)
            {
                return ServiceResult<List<ScheduleResponseDto>>.Fail(404, "CAREER_NOT_FOUND", "La carrera especificada no existe.");
            }


            return await GetSchedulesCoreAsync(careerId, day);
        }


        private async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesCoreAsync(int careerId, string day)
        {
            var schedules = await _scheduleRepository.GetSchedulesByCareerAndDayAsync(careerId, day);

            var resultDto = schedules.Select(s => new ScheduleResponseDto
            {
                Id = s.Id,
                Subject = s.Subject,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                DurationHours = s.DurationHours,
                Professor = s.Professor,
                Classroom = s.Classroom
            }).ToList();

            return ServiceResult<List<ScheduleResponseDto>>.Ok(resultDto);
        }


        public async Task<ServiceResult<ScheduleResponseDto>> CreateScheduleAsync(ScheduleCreateDto dto)
        {

            var careerExists = await _careerRepository.CareerExistsAsync(dto.CareerId);
            if (!careerExists)
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_FOUND",
                    "La carrera especificada no existe."
                );
            }


            string cleanTime = dto.StartTime.Replace(" pm", "", StringComparison.OrdinalIgnoreCase)
                                            .Replace(" am", "", StringComparison.OrdinalIgnoreCase)
                                            .Trim();

            if (!TimeSpan.TryParse(cleanTime, out TimeSpan parsedTime))
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_TIME_FORMAT",
                    "El formato de la hora de inicio es inválido."
                );
            }

            var newSchedule = new Schedule
            {
                CareerId = dto.CareerId,
                Subject = dto.Subject,
                Day = dto.Day,
                StartTime = parsedTime,
                DurationHours = (decimal)dto.DurationHours,
                Professor = dto.Professor,
                Classroom = dto.Classroom
            };


            await _scheduleRepository.AddNewScheduleAsync(newSchedule);
            var schedule = await _scheduleRepository.GetScheduleByIdAsync(newSchedule.Id);

            if(schedule == null)
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "SCHEDULE_NOT_FOUND",
                    "El horario no se registró en nuestra base de datos."
                );
            }

            var responseDto = new ScheduleResponseDto
            {
                Id = schedule.Id,
                Subject = schedule.Subject,
                StartTime = schedule.StartTime.ToString(),
                DurationHours = schedule.DurationHours,
                Professor = schedule.Professor,
                Classroom = schedule.Classroom
            };

            return ServiceResult<ScheduleResponseDto>.Ok(responseDto);
        }
    }
}