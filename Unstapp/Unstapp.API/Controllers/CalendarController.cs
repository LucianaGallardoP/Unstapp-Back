using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.DTOs.Calendar;
using Unstapp.Application.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;
        private readonly ICalendarEventReminderService _calendarEventReminderService;

        public CalendarController(
            ICalendarService calendarService,
            ICalendarEventReminderService calendarEventReminderService)
        {
            _calendarService = calendarService;
            _calendarEventReminderService = calendarEventReminderService;
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });
            }

            var result = await _calendarService.GetEventsByRangeAsync(start, end, currentUserId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost("events")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateCalendarEventDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized(
                    new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Code = "INVALID_TOKEN",
                        Message = "Token inválido."
                    }
                );

            var result = await _calendarService.CreateEventAsync(dto, currentUserId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return StatusCode(StatusCodes.Status201Created, result.Data);
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetTodayEvents()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });
            }

            var result = await _calendarService.GetTodayEventsAsync(currentUserId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet("events/day")]
        public async Task<IActionResult> GetEventsByDay([FromQuery] DateTime date)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });
            }

            var result = await _calendarService.GetEventsByDayAsync(date, currentUserId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpDelete("events/{eventId:int}")]
        [Authorize(Roles = "Administracion")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            var result = await _calendarService.DeleteEventAsync(eventId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return NoContent();
        }

        [HttpPatch("events/{eventId:int}/reminder")]
        public async Task<IActionResult> UpdateEventReminder(int eventId, [FromBody] UpdateCalendarEventReminderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Tóken inválido."
                });

            var result = await _calendarEventReminderService.UpdateReminderAsync(
                currentUserId,
                eventId,
                dto.Enabled
            );

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(new
            {
                eventId,
                reminderEnabled = result.Data
            });
        }
    }
}
