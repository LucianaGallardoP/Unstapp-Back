using AutoMapper;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs;
using Unstapp.Application.DTOs.Horarios;
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
        private readonly IMapper _mapper;

        public ScheduleService(
            ICareerRepository careerRepository,
            IScheduleRepository scheduleRepository,
            IMapper mapper
        )
        {
            _careerRepository = careerRepository;
            _scheduleRepository = scheduleRepository;
            _mapper = mapper;
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

            var newSchedule = _mapper.Map<Schedule>(dto);
            newSchedule.StartTime = parsedTime;

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

            var response = _mapper.Map<ScheduleResponseDto>(schedule);

            return ServiceResult<ScheduleResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<ScheduleResponseDto>> UpdateScheduleAsync(
            int id,
            ScheduleUpdateDto dto)
        {
            if(id <= 0)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_SCHEDULE_ID",
                    "El ID del horario no es válido."
                );

            var schedule = await _scheduleRepository.GetScheduleByIdAsync(id);

            if (schedule == null)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "SCHEDULE_NOT_FOUND",
                    "Horario no encontrado."
                );

            if(dto.CareerId.HasValue)
            {
                if (dto.CareerId.Value <= 0)
                    return ServiceResult<ScheduleResponseDto>.Fail(
                        StatusCodes.Status400BadRequest,
                        "INVALID_CAREER_ID",
                        "La carrera no es válida."
                    );

                var careerExists = await _careerRepository.CareerExistsAsync(dto.CareerId.Value);

                if(!careerExists)
                    return ServiceResult<ScheduleResponseDto>.Fail(
                        StatusCodes.Status404NotFound,
                        "CAREER_NOT_FOUND",
                        "La carrera especificada no existe."
                    );
            }

            if (dto.Subject != null && string.IsNullOrWhiteSpace(dto.Subject))
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "SUBJECT_REQUIRED",
                    "La materia no puede estar vacía."
                );
            }

            if (dto.Day != null && string.IsNullOrWhiteSpace(dto.Day))
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "DAY_REQUIRED",
                    "El día no puede estar vacío."
                );
            }

            if (dto.DurationHours.HasValue && dto.DurationHours.Value <= 0)
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_DURATION",
                    "La duración debe ser mayor a cero."
                );
            }

            if (dto.StartTime != null)
            {
                if (string.IsNullOrWhiteSpace(dto.StartTime))
                {
                    return ServiceResult<ScheduleResponseDto>.Fail(
                        StatusCodes.Status400BadRequest,
                        "INVALID_TIME_FORMAT",
                        "La hora de inicio no puede estar vacía."
                    );
                }

                string cleanTime = dto.StartTime
                    .Replace(" pm", "", StringComparison.OrdinalIgnoreCase)
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

                schedule.StartTime = parsedTime;
            }

            _mapper.Map(dto, schedule);

            await _scheduleRepository.UpdateScheduleAsync(schedule);

            var updatedSchedule = await _scheduleRepository.GetScheduleByIdAsync(schedule.Id);

            if(updatedSchedule == null)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "SCHEDULE_NOT_FOUND",
                    "El horario no se pudo actualizar correctamente en nuestra base de datos."
                );

            var response = _mapper.Map<ScheduleResponseDto>(updatedSchedule);

            return ServiceResult<ScheduleResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<bool>> DeleteScheduleAsync(int id)
        {
            if (id <= 0)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_SCHEDULE_ID",
                    "El ID del horario no es válido."
                );

            var schedule = await _scheduleRepository.GetScheduleByIdAsync(id);

            if (schedule == null)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "SCHEDULE_NOT_FOUND",
                    "Horario no encontrado."
                );

            await _scheduleRepository.SoftDeleteAsync(schedule);

            return ServiceResult<bool>.Ok(true);
        }
    }
}