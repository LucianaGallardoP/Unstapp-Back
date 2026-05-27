using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
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

                var result = await _postService.CreateAsync(userId, dto);

                if (!result.Success)
                    return StatusCode(result.Error!.StatusCode, result.Error);

                return Ok(result.Data);
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
