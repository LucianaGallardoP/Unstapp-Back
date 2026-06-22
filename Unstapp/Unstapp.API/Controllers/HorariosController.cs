using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;

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
        public async Task<IActionResult> GetHorariosDelDia([FromQuery] string dia, [FromQuery] int? careerId)
        {
            if (string.IsNullOrWhiteSpace(dia))
            {
                return BadRequest(new { mensaje = "El parámetro 'dia' es obligatorio." });
            }

            if (careerId.HasValue && careerId.Value > 0)
            {

                if (!User.IsInRole("Admin"))
                {

                    return StatusCode(403, new { mensaje = "Acceso denegado: Se requieren permisos de Administrador." });
                }

                var adminResult = await _scheduleService.GetSchedulesByCareerAsync(careerId.Value, dia);

                if (!adminResult.Success)
                {
                    return StatusCode(adminResult.Error!.StatusCode, new { mensaje = adminResult.Error.Message });
                }

                return Ok(adminResult.Data);
            }


            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { mensaje = "Token de usuario inválido o expirado." });
            }

            var studentResult = await _scheduleService.GetSchedulesByDayAsync(userId, dia);

            if (!studentResult.Success)
            {
                return StatusCode(studentResult.Error!.StatusCode, new { mensaje = studentResult.Error.Message });
            }

            return Ok(studentResult.Data);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateHorario([FromBody] ScheduleCreateDto dto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _scheduleService.CreateScheduleAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.Error!.StatusCode, new { mensaje = result.Error.Message });
            }


            return StatusCode(201, result.Data);
        }
    }


}