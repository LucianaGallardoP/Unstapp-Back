using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Data;
using Unstapp.Infrastructure.Entities;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _context;

        public ScheduleService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByDayAsync(int userId, string day)
        {

            var userCareer = await _context.UserCareers
                .FirstOrDefaultAsync(uc => uc.UserId == userId);

            if (userCareer == null)
            {
                return ServiceResult<List<ScheduleResponseDto>>.Fail(404, "CAREER_NOT_FOUND", "El usuario no tiene una carrera asignada.");
            }


            return await GetSchedulesCoreAsync(userCareer.CareerId, day);
        }


        public async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByCareerAsync(int careerId, string day)
        {

            var careerExists = await _context.Careers.AnyAsync(c => c.CareerId == careerId);
            if (!careerExists)
            {
                return ServiceResult<List<ScheduleResponseDto>>.Fail(404, "CAREER_NOT_FOUND", "La carrera especificada no existe.");
            }


            return await GetSchedulesCoreAsync(careerId, day);
        }


        private async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesCoreAsync(int careerId, string day)
        {
            var schedules = await _context.Schedules
                .Where(s => s.CareerId == careerId && s.Day.ToLower() == day.ToLower())
                .OrderBy(s => s.StartTime)
                .ToListAsync();

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

            var careerExists = await _context.Careers.AnyAsync(c => c.CareerId == dto.CareerId);
            if (!careerExists)
            {
                return ServiceResult<ScheduleResponseDto>.Fail(404, "CAREER_NOT_FOUND", "La carrera especificada no existe.");
            }


            string cleanTime = dto.StartTime.Replace(" pm", "", StringComparison.OrdinalIgnoreCase)
                                            .Replace(" am", "", StringComparison.OrdinalIgnoreCase)
                                            .Trim();

            if (!TimeSpan.TryParse(cleanTime, out TimeSpan parsedTime))
            {
                return ServiceResult<ScheduleResponseDto>.Fail(400, "INVALID_TIME_FORMAT", "El formato de la hora de inicio es inválido.");
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


            _context.Schedules.Add(newSchedule);
            await _context.SaveChangesAsync();


            var responseDto = new ScheduleResponseDto
            {
                Id = newSchedule.Id,
                Subject = newSchedule.Subject,
                StartTime = cleanTime,
                DurationHours = newSchedule.DurationHours,
                Professor = newSchedule.Professor,
                Classroom = newSchedule.Classroom
            };

            return ServiceResult<ScheduleResponseDto>.Ok(responseDto);
        }
    }
}