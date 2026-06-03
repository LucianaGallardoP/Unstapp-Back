using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Application.Services;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/posts/{postId:int}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentsService;

        public CommentsController(ICommentService commentService)
        {
            _commentsService = commentService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllByPost(int postId)
        {
            var commentsDto = await _commentsService.GetAllByPostAsync(postId);

            if(commentsDto == null)
                return NotFound(new { message = "Post no encontrado." });

            return Ok(commentsDto);
        }
        [HttpPost]
        public async Task<IActionResult> Create(int postId, [FromBody] CreateCommentDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized(new { message = "Token Inválido." });

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Identificador de Usuario Inválido." });

            var comment = await _commentsService.AddAsync(postId, userId, dto);

            return Ok(comment);
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
                    Message = "Tóken inválido."
                });

            var result = await _commentsService.DeleteAsync(id, currentUserId);

            if (!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return NoContent();
        }
    }
}
