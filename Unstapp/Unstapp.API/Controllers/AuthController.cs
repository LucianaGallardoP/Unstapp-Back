using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.DTOs;
using Unstapp.Application.DTOs.Auth;
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

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
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
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost("set-initial-password")]
        public async Task<IActionResult> SetInitialPassword(SetInitialPasswordRequestDto dto)
        {
            var result = await _authService.SetInitialPasswordAsync(dto);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(new
            {
                message = "Si el DNI existe y tiene un email asociado, se enviará un enlace de recuperación."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(new
            {
                message = "Contraseña actualizada correctamente."
            });
        }
    }
}
