using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarEventRepository _calendarEventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CalendarService(
            ICalendarEventRepository calendarEventRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _calendarEventRepository = calendarEventRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<CalendarEventsResponseDto>> GetEventsByRangeAsync(
            DateTime start,
            DateTime end)
        {
            if (start > end)
                return ServiceResult<CalendarEventsResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_DATE_RANGE",
                    "La fecha de inicio no puede ser mayor que la fecha de fin."
                );

            var startDateArgentina = start.Date;
            var endExclusiveArgentina = end.Date.AddDays(1);

            var startUtc = ConvertArgentinaLocalToUtc(startDateArgentina);
            var endUtc = ConvertArgentinaLocalToUtc(endExclusiveArgentina);

            var events = await _calendarEventRepository.GetEventsByRangeAsync(startUtc, endUtc);
            var eventsDto = _mapper.Map<List<CalendarEventDto>>(events);

            var response = new CalendarEventsResponseDto
            {
                Events = eventsDto,

                TypeCounts = new CalendarTypeCountsDto
                {
                    Examenes = events.Count(e => e.Type == CalendarEventType.Examen),
                    Clases = events.Count(e => e.Type == CalendarEventType.Clase),
                    Eventos = events.Count(e => e.Type == CalendarEventType.Evento),
                    Feriados = events.Count(e => e.Type == CalendarEventType.Feriado)
                },
                EventDays = events
                    .Select(e =>
                    {
                        var argentinaTimeZone = GetArgentinaTimeZone();
                        var argentinaDate = TimeZoneInfo.ConvertTimeFromUtc(e.StartDate, argentinaTimeZone);
                        return DateOnly.FromDateTime(argentinaDate);
                    })
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList()
            };

            return ServiceResult<CalendarEventsResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<CalendarEventDto>> CreateEventAsync(CreateCalendarEventDto dto, int currentUserId)
        {
            var roles = await _userRepository.GetRoleNameByUserIdAsync(currentUserId);

            var canCreateCalendarEvent = CanCreateCalendarEvent(roles, dto.Type);

            if (!canCreateCalendarEvent)
                return ServiceResult<CalendarEventDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "FORBIDDEN_CALENDAR_EVENT_CREATE",
                    "No tienes permisos para crear este tipo de evento en el calendario."
                );

            if (string.IsNullOrWhiteSpace(dto.Title))
                return ServiceResult<CalendarEventDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TITLE_REQUIRED",
                    "El título es obligatorio."
                );

            if (dto.StartDate > dto.EndDate)
                return ServiceResult<CalendarEventDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_DATE_RANGE",
                    "La fecha de inicio no puede ser mayor a la fecha de fin."
                );

            var startUtc = ConvertArgentinaLocalToUtc(dto.StartDate);
            var endUtc = ConvertArgentinaLocalToUtc(dto.EndDate);

            var calendarEvent = new CalendarEvent
            {
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                Type = dto.Type,
                StartDate = startUtc,
                EndDate = endUtc,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _calendarEventRepository.AddAsync(calendarEvent);

            var responseDto = _mapper.Map<CalendarEventDto>(calendarEvent);

            return ServiceResult<CalendarEventDto>.Ok(responseDto);
        }

        public async Task<ServiceResult<List<CalendarEventDto>>> GetTodayEventsAsync()
        {
            var argentinaNow = GetArgentinaNow();

            var todayArgentina = argentinaNow.Date;

            var result = await GetEventsByRangeAsync(todayArgentina, todayArgentina);

            if (!result.Success)
                return ServiceResult<List<CalendarEventDto>>.Fail(
                    result.Error!.StatusCode,
                    result.Error.Code,
                    result.Error.Message
                );

            return ServiceResult<List<CalendarEventDto>>.Ok(result.Data!.Events);
        }

        private static bool CanCreateCalendarEvent(List<string> roles, CalendarEventType eventType)
        {
            if(HasAdministrativeRoles(roles))
                return true;

            if(HasTeacherRoles(roles))
            {
                return eventType == CalendarEventType.Clase ||
                    eventType == CalendarEventType.Examen;
            }
            return false;
        }

        private static bool HasAdministrativeRoles(List<string> roles)
        {
            var administrativeRoles = new[]
            {
                "Admin",
                "Administrador",
                "Administracion",
                "Administrativo"
            };

            return roles.Any(role =>
                administrativeRoles.Any(allowed =>
                    allowed.Equals(role, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool HasTeacherRoles(List<string> roles)
        {
            var teacherRoles = new[]
            {
                "Docente",
                "Profesor"
            };
            return roles.Any(role =>
                teacherRoles.Any(allowed =>
                    allowed.Equals(role, StringComparison.OrdinalIgnoreCase)));
        }

        private static TimeZoneInfo GetArgentinaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            }
        }

        private static DateTime ConvertArgentinaLocalToUtc(DateTime argentinaLocalDateTime)
        {
            var argentinaTimeZone = GetArgentinaTimeZone();
            var unespecifiedTime = DateTime.SpecifyKind(argentinaLocalDateTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(unespecifiedTime, argentinaTimeZone);
        }

        private static DateTime GetArgentinaNow()
        {
            var argentinaTimeZone = GetArgentinaTimeZone();

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone);
        }
    }
}
