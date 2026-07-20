using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Interfaces;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/posts/{postId:int}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentsService;
        private readonly IModerationService _moderationService;

        public CommentsController(
            ICommentService commentService,
            IModerationService moderationService)
        {
            _commentsService = commentService;
            _moderationService = moderationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllByPost(int postId)
        {
            var result = await _commentsService.GetAllByPostAsync(postId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int postId, [FromBody] CreateCommentDto dto)
        {
            var moderationResult = await _moderationService.ModerateContentAsync(dto.Content);

            if(!moderationResult.IsApproved)
                return BadRequest(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Code = moderationResult.Code,
                    Message = moderationResult.Message
                });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });

            var result = await _commentsService.AddAsync(postId, userId, dto);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return StatusCode(StatusCodes.Status201Created, result.Data);
        }

        [HttpDelete("/api/comments/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = "INVALID_TOKEN",
                    Message = "Token inválido."
                });

            var result = await _commentsService.DeleteAsync(id, currentUserId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return NoContent();
        }
    }
}
