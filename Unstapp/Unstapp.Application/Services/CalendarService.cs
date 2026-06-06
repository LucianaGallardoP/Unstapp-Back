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

            var events = await _calendarEventRepository.GetEventsByRangeAsync(start, end);
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
                    .Select(e => DateOnly.FromDateTime(e.StartDate))
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList()
            };

            return ServiceResult<CalendarEventsResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<CalendarEventDto>> CreateEventAsync(CreateCalendarEventDto dto, int currentUserId)
        {
            var roles = await _userRepository.GetRoleNameByUserIdAsync(currentUserId);

            var canCreateCalendarEvent = CanCreateCalendarEvent(roles);

            if (!canCreateCalendarEvent)
                return ServiceResult<CalendarEventDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "FORBIDDEN_CALENDAR_EVENT_CREATE",
                    "No tienes permisos para crear eventos en el calendario."
                );

            if (string.IsNullOrWhiteSpace(dto.Title))
                return ServiceResult<CalendarEventDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "TITLE_REQUIRED",
                    "El título es obligatorio."
                );

            if(dto.StartDate > dto.EndDate)
                return ServiceResult<CalendarEventDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_DATE_RANGE",
                    "La fecha de inicio no puede ser mayor a la fecha de fin."
                );

            var calendarEvent = new CalendarEvent
            {
                Title = dto.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description,
                Type = dto.Type,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _calendarEventRepository.AddAsync(calendarEvent);

            var responseDto = _mapper.Map<CalendarEventDto>(calendarEvent);

            return ServiceResult<CalendarEventDto>.Ok(responseDto);
        }

        private static bool CanCreateCalendarEvent(List<string> roles)
        {
            var allowedRoles = new[]
            {
                "Admin",
                "Administrador",
                "Administracion",
                "Administrativo"
            };

            return roles.Any(role =>
                allowedRoles.Any(allowed =>
                    allowed.Equals(role, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
