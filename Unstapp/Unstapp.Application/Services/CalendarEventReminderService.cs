using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Unstapp.Application.Interfaces;
using Unstapp.Shared.DTOs.WhatsApp;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Interfaces;
using Unstapp.Shared.Helpers;
using Microsoft.Extensions.Configuration;

namespace Unstapp.Application.Services
{
    public class CalendarEventReminderService : ICalendarEventReminderService
    {

        private readonly ICalendarEventRepository _calendarEventRepository;
        private readonly ICalendarEventReminderRepository _calendarEventReminderRepository;
        private readonly INotificationService _notificationService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<CalendarEventReminderService> _logger;
        private readonly IConfiguration _config;

        public CalendarEventReminderService(
            ICalendarEventRepository calendarEventRepository,
            ICalendarEventReminderRepository calendarEventReminderRepository,
            INotificationService notificationService,
            IWhatsAppService whatsAppService,
            ILogger<CalendarEventReminderService> logger,
            IConfiguration config)
        {
            _calendarEventRepository = calendarEventRepository;
            _calendarEventReminderRepository = calendarEventReminderRepository;
            _notificationService = notificationService;
            _whatsAppService = whatsAppService;
            _logger = logger;
            _config = config;
        }
        
        public async Task<ServiceResult<bool>> UpdateReminderAsync(
            int userId,
            int calendarEventId,
            bool enabled)
        {
            if (userId <= 0)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_USER_ID",
                    "El usuario no es válido."
                );

            if(calendarEventId <= 0)
                return ServiceResult<bool>.Fail(
                StatusCodes.Status400BadRequest,
                "INVALID_EVENT_ID",
                "El evento no es válido."
            );

            var calendarEvent = await _calendarEventRepository.GetByIdAsync(calendarEventId);

            if(calendarEvent == null)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "EVENT_NOT_FOUND",
                    "El evento no existe."
                );

            var reminder = await _calendarEventReminderRepository.GetReminderAsync(userId, calendarEventId);

            if(reminder == null)
            {
                reminder = new CalendarEventReminder
                {
                    UserId = userId,
                    CalendarEventId = calendarEventId,
                    IsEnabled = enabled,
                    CreatedAt = DateTime.UtcNow,
                    SentAt = null,
                    AppNotificationSent = false,
                    WhatsAppSent = false
                };

                await _calendarEventReminderRepository.AddAsync(reminder);
            }
            else
            {
                reminder.IsEnabled = enabled;

                if(enabled)
                {
                    reminder.SentAt = null;
                    reminder.AppNotificationSent = false;
                    reminder.WhatsAppSent = false;
                }

                await _calendarEventReminderRepository.UpdateAsync(reminder);
            }

            return ServiceResult<bool>.Ok(enabled);
        }

        public async Task ProcessDueRemindersAsync(CancellationToken cancellationToken = default)
        {
            var todayArgentina = DateHelper.GetArgentinaToday();

            var reminderWindowDays = _config.GetValue<int?>("CalendarReminders:WindowDays") ?? 2;

            var maxEventDate = todayArgentina.AddDays(reminderWindowDays);

            _logger.LogInformation(
                "Procesando recordatorios para eventos del día {maxEventDate}.",
                maxEventDate
            );

            var reminders = await _calendarEventReminderRepository
                .GetDueRemindersAsync(maxEventDate);

            _logger.LogInformation(
                "Recordatorios encontrados: {ReminderCount}.",
                reminders.Count
            );

            var uniqueReminders = reminders
                .GroupBy(r => new { r.UserId, r.CalendarEventId })
                .Select(g => g.First())
                .ToList();

            foreach (var reminder in uniqueReminders)
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("ProcessDueRemindersAsync was cancelled.");
                    break;
                }

                _logger.LogInformation(
                    "Enviando recordatorio del evento {CalendarEventId} al usuario {UserId}.",
                    reminder.CalendarEventId,
                    reminder.UserId
                );

                var user = reminder.User;
                var calendarEvent = reminder.CalendarEvent;

                var appNotificationSent = false;
                var whatsAppSent = false;

                try
                {
                    await _notificationService.CreateCalendarEventReminderNotificationAsync(
                        user.UserId,
                        calendarEvent.CalendarEventId,
                        calendarEvent.Title,
                        calendarEvent.StartDate
                    );

                    appNotificationSent = true;

                    if (user.WhatsAppNotificationsEnabled && !string.IsNullOrWhiteSpace(user.PhoneNumber))
                    {
                        var eventStartArgentina = DateHelper.ConvertUtcToArgentina(calendarEvent.StartDate);

                        whatsAppSent = await _whatsAppService.SendCalendarEventReminderTemplateAsync(
                            new CalendarEventReminderWhatsAppDto
                            {
                                ToPhoneNumber = user.PhoneNumber,
                                StudentName = $"{user.Name} {user.LastName}".Trim(),
                                EventTitle = calendarEvent.Title,
                                EventType = calendarEvent.Type.ToString(),
                                EventDate = eventStartArgentina.ToString("dd/MM/yyyy"),
                                EventTime = eventStartArgentina.ToString("HH:mm"),
                                Description = string.IsNullOrWhiteSpace(calendarEvent.Description)
                                    ? "-"
                                    : calendarEvent.Description
                            });
                    }

                    await _calendarEventReminderRepository.MarkAsSentAsync(reminder, appNotificationSent, whatsAppSent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error enviando recordatorio del evento {CalendarEventId} al usuario {UserId}.",
                        calendarEvent.CalendarEventId,
                        user.UserId
                    );

                    if(appNotificationSent)
                    {
                        await _calendarEventReminderRepository.MarkAsSentAsync(reminder, appNotificationSent, whatsAppSent);
                    }
                }

                await _notificationService.CreateCalendarEventReminderNotificationAsync(
                    user.UserId,
                    calendarEvent.CalendarEventId,
                    calendarEvent.Title,
                    calendarEvent.StartDate
                );
            }
        }
    }
}
