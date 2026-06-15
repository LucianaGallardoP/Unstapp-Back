using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Unstapp.Application.Interfaces;
using Unstapp.Application.Services;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IPostService _postService;
        private readonly IUserService _userService;

        public UsersController(
            IProfileService profileService,
            IPostService postService,
            IUserService userService)
        {
            _profileService = profileService;
            _postService = postService;
            _userService = userService;
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

        [HttpGet("{id:int}/posts")]
        public async Task<IActionResult> GetPostsByUser(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });

            var result = await _postService.GetPostsByUserAsync(id);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpGet("me/context")]
        public async Task<IActionResult> GetMyContext()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token Inválido."
                });
            
            var result = await _userService.GetUserContextAsync(userId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}
