using AutoMapper;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs.Calendar;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Helpers;

namespace Unstapp.Application.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarEventRepository _calendarEventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICalendarEventReminderRepository _calendarEventReminderRepository;
        private readonly ICareerRepository _careerRepository;

        public CalendarService(
            ICalendarEventRepository calendarEventRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ICalendarEventReminderRepository calendarEventReminderRepository,
            ICareerRepository careerRepository)
        {
            _calendarEventRepository = calendarEventRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _calendarEventReminderRepository = calendarEventReminderRepository;
            _careerRepository = careerRepository;
        }

        public async Task<ServiceResult<CalendarEventsResponseDto>> GetEventsByRangeAsync(
            DateTime start,
            DateTime end,
            int currentUserId)
        {
            if (start > end)
                return ServiceResult<CalendarEventsResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_DATE_RANGE",
                    "La fecha de inicio no puede ser mayor que la fecha de fin."
                );

            var startDateArgentina = start.Date;
            var endExclusiveArgentina = end.Date.AddDays(1);

            var startUtc = DateHelper.ConvertArgentinaLocalToUtc(startDateArgentina);
            var endUtc = DateHelper.ConvertArgentinaLocalToUtc(endExclusiveArgentina);

            var roles = await _userRepository.GetRoleNameByUserIdAsync(currentUserId);

            var includeAllCareers = RoleHelper.IsAdmin(roles);

            var userCareerIds = includeAllCareers
                ? new List<int>()
                : await _userRepository.GetCareerIdsByUserIdAsync(currentUserId);

            var events = await _calendarEventRepository.GetEventsByRangeAsync(
                startUtc,
                endUtc,
                userCareerIds,
                includeAllCareers
            );

            var eventIds = events.Select(e => e.CalendarEventId).ToList();
            var enabledReminderEventIds = await _calendarEventReminderRepository.GetEnabledReminderEventIdsAsync(currentUserId, eventIds);
            
            var enabledReminderEventIdsSet = enabledReminderEventIds.ToHashSet();

            var eventsDto = _mapper.Map<List<CalendarEventDto>>(events);

            foreach(var eventDto in eventsDto)
            {
                eventDto.ReminderEnabledForCurrentUser = enabledReminderEventIdsSet.Contains(eventDto.CalendarEventId);
            }

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
                        var argentinaTimeZone = DateHelper.GetArgentinaTimeZone();
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

            var isAdmin = RoleHelper.IsAdmin(roles);
            var isProffesor = RoleHelper.IsProffesor(roles);

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

            var startUtc = DateHelper.ConvertArgentinaLocalToUtc(dto.StartDate);
            var endUtc = DateHelper.ConvertArgentinaLocalToUtc(dto.EndDate);


            var requestedCareerIds = dto.CareerIds.Distinct().ToList() ?? new List<int>();

            if(requestedCareerIds.Any(id => id <= 0))
                return ServiceResult<CalendarEventDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_CAREER_ID",
                    "Los IDs de carrera no son válidos."
                );

            var eventCareerIds = new List<int>();

            if(isProffesor && !isAdmin)
            {
                var proffesorCareerIds = await _userRepository.GetCareerIdsByUserIdAsync(currentUserId);

                proffesorCareerIds = proffesorCareerIds.Distinct().ToList();

                if(proffesorCareerIds.Count == 0)
                    return ServiceResult<CalendarEventDto>.Fail(
                        StatusCodes.Status400BadRequest,
                        "TEACHER_WITHOUT_CAREERS",
                        "El docente no tiene carreras asignadas."
                    );

                if(requestedCareerIds.Count == 0)
                {
                    eventCareerIds = proffesorCareerIds;
                }
                else
                {
                    var unauthorizedCareerIds = requestedCareerIds.Except(proffesorCareerIds).ToList();

                    if(unauthorizedCareerIds.Count > 0)
                        return ServiceResult<CalendarEventDto>.Fail(
                            StatusCodes.Status403Forbidden,
                            "CAREER_NOT_ASSIGNED_TO_TEACHER",
                            "No podés crear la clase para una carrera que no tenés asignada."
                        );

                    eventCareerIds = requestedCareerIds;
                }
            }
            else
            {
                eventCareerIds = requestedCareerIds;

                foreach(var careerId in eventCareerIds)
                {
                    var careerExists = await _careerRepository.CareerExistsAsync(careerId);

                    if (!careerExists)
                        return ServiceResult<CalendarEventDto>.Fail(
                            StatusCodes.Status404NotFound,
                            "CAREER_NOT_FOUND",
                            $"La carrera con ID {careerId} no existe."
                        );
                }
            }

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

            await _calendarEventRepository.AddWithCareerAsync(calendarEvent, eventCareerIds);

            var responseDto = _mapper.Map<CalendarEventDto>(calendarEvent);
            responseDto.ReminderEnabledForCurrentUser = false;

            return ServiceResult<CalendarEventDto>.Ok(responseDto);
        }

        public async Task<ServiceResult<List<CalendarEventDto>>> GetTodayEventsAsync(int currentUserId)
        {
            var argentinaNow = DateHelper.GetArgentinaNow();

            var todayArgentina = argentinaNow.Date;

            var result = await GetEventsByRangeAsync(todayArgentina, todayArgentina, currentUserId);

            if (!result.Success)
                return ServiceResult<List<CalendarEventDto>>.Fail(
                    result.Error!.StatusCode,
                    result.Error.Code,
                    result.Error.Message
                );

            return ServiceResult<List<CalendarEventDto>>.Ok(result.Data!.Events);
        }

        public async Task<ServiceResult<List<CalendarEventDto>>> GetEventsByDayAsync(DateTime date, int currentUserId)
        {
            var dayArgentina = date.Date;

            var result = await GetEventsByRangeAsync(dayArgentina, dayArgentina, currentUserId);

            if (!result.Success)
                return ServiceResult<List<CalendarEventDto>>.Fail(
                    result.Error!.StatusCode,
                    result.Error.Code,
                    result.Error.Message
                );

            return ServiceResult<List<CalendarEventDto>>.Ok(result.Data!.Events);
        }

        public async Task<ServiceResult<bool>> DeleteEventAsync(int eventId)
        {
            if (eventId < 0)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_EVENT_ID",
                    "El ID del evento no es válido."
                );

            var calendarEvent = await _calendarEventRepository.GetByIdAsync(eventId);

            if (calendarEvent == null)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "EVENT_NOT_FOUND",
                    "Evento no encontrado."
                );

            await _calendarEventRepository.DeleteEventAsync(calendarEvent);

            return ServiceResult<bool>.Ok(true);
        }

        private static bool CanCreateCalendarEvent(List<string> roles, CalendarEventType eventType)
        {
            if (RoleHelper.IsAdmin(roles))
                return true;

            if (RoleHelper.IsProffesor(roles))
                return eventType == CalendarEventType.Clase;

            return false;
        }
    }
}
