using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Interfaces;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IModerationService _moderationService;

        public PostsController(
            IPostService postService,
            IModerationService moderationService)
        {
            _postService = postService;
            _moderationService = moderationService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PostFilter filter = PostFilter.Todos)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });

            var result = await _postService.GetAllAsync(userId, filter);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreatePostDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Code = "INVALID_TOKEN",
                        Message = "Token inválido."
                    });

                var moderationResult = await _moderationService.ModeratePostAsync(dto.Content);

                if (!moderationResult.IsApproved)
                    return BadRequest(new ApiErrorResponse
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Code = moderationResult.Code,
                        Message = moderationResult.Message ?? "Tu publicación contiene lenguaje que infringe las normas de la comunidad."
                    });

                var result = await _postService.CreateAsync(userId, dto);

                if (!result.Success)
                    return StatusCode(result.Error!.StatusCode, result.Error);

                return StatusCode(StatusCodes.Status201Created, result.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("{postId:int}")]
        public async Task<IActionResult> GetById(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });
            }

            var result = await _postService.GetByIdAsync(userId, postId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete("{postId:int}")]
        public async Task<IActionResult> Delete(int postId)
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

            var result = await _postService.DeleteAsync(postId, currentUserId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return NoContent();
        }
    }
}
