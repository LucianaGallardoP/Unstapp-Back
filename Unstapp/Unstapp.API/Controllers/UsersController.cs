using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Unstapp.Application.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public UsersController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpPost("{id:int}/follow")]
        public async Task<IActionResult> ToggleFollow(int id)
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

            var result = await _profileService.ToggleFollowAsync(currentUserId, id);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
