using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
            {
                return Unauthorized(new { message = "Usuario o Contraseña Incorrectos." });
            }

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            await _authService.RegisterAsync(dto);
            return Ok();
        }

        [HttpPost("verify-first-time")]
        public async Task<IActionResult> VerifyFirstTime(VerifyFirstTimeRequestDto dto)
        {
            var result = await _authService.VerifyFirstTimeAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.Error!.StatusCode, result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPost("set-initial-password")]
        public async Task<IActionResult> SetInitialPassword(SetInitialPasswordRequestDto dto)
        {
            var result = await _authService.SetInitialPasswordAsync(dto);

            if (!result.Success)
            {
                return StatusCode(result.Error!.StatusCode, result.Error);
            }

            return Ok(result.Data);
        }
    }
}
