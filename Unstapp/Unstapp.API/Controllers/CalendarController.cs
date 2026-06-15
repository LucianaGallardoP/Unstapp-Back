using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.DTOs;
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

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            var result = await _calendarService.GetEventsByRangeAsync(start, end);

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

            return CreatedAtAction(
                nameof(GetEvents),
                new
                {
                    start = result.Data!.StartDate,
                    end = result.Data.EndDate
                },
                result.Data
            );
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetTodayEvents()
        {
            var result = await _calendarService.GetTodayEventsAsync();

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet("events/day")]
        public async Task<IActionResult> GetEventsByDay([FromQuery] DateTime date)
        {
            var result = await _calendarService.GetEventsByDayAsync(date);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
