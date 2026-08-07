using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.DTOs.Horarios;
using Unstapp.Application.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Helpers;

namespace Unstapp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/horarios")]
    public class HorariosController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public HorariosController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHorariosDelDia(
            [FromQuery] string dia,
            [FromQuery] int? careerId,
            [FromQuery] int? year)
        {
            if (string.IsNullOrWhiteSpace(dia))
                return BadRequest(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Code = "DAY_REQUIRED",
                    Message = "El parámetro 'dia' es obligatorio."
                });

            if (!year.HasValue)
                return BadRequest(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Code = "YEAR_REQUIRED",
                    Message = "El parámetro 'year' es obligatorio."
                });

            if (year.Value <= 0)
                return BadRequest(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Code = "INVALID_YEAR",
                    Message = "El año no es válido."
                });

            if (careerId.HasValue && careerId.Value > 0)
            {
                if (!User.IsInRole("Administracion") && !User.IsInRole("Docente"))
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new ApiErrorResponse
                        {
                            StatusCode = StatusCodes.Status403Forbidden,
                            Code = "ACCESS_DENIED",
                            Message = "Acceso denegado: Se requiere rol de Administracion o Docente."
                        }
                    );

                var adminResult = await _scheduleService.GetSchedulesByCareerAsync(
                    careerId.Value,
                    dia,
                    year.Value
                );

                if (!adminResult.Success)
                    return StatusCode(adminResult.Error!.StatusCode, adminResult.Error);

                return Ok(adminResult.Data);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });

            var studentResult = await _scheduleService.GetSchedulesByDayAsync(userId, dia, year.Value);

            if (!studentResult.Success)
                return StatusCode(studentResult.Error!.StatusCode, studentResult.Error);

            return Ok(studentResult.Data);
        }


        [HttpPost]
        [Authorize(Roles = "Administracion")]
        public async Task<IActionResult> CreateHorario([FromBody] ScheduleCreateDto dto)
        {
            var result = await _scheduleService.CreateScheduleAsync(dto);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return StatusCode(StatusCodes.Status201Created, result.Data);
        }

        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Administracion")]
        public async Task<IActionResult> UpdateHorario(int id, [FromBody] ScheduleUpdateDto dto)
        {
            var result = await _scheduleService.UpdateScheduleAsync(id, dto);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administracion")]
        public async Task<IActionResult> DeleteHorario(int id)
        {
            var result = await _scheduleService.DeleteScheduleAsync(id);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return NoContent();
        }

        [HttpPost("import")]
        [Authorize(Roles = "Administracion")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportHorarios([FromForm] ScheduleImportRequestDto requestDto)
        {
            var result = await _scheduleService.ImportSchedulesAsync(requestDto.File);

            if(!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}