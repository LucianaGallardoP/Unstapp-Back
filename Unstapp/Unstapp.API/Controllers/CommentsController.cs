using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Application.Services;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/posts/{postId:int}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentsService;

        public CommentsController(ICommentService commentService)
        {
            _commentsService = commentService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllByPost(int postId)
        {
            var commentsDto = await _commentsService.GetAllByPostAsync(postId);

            if(commentsDto == null)
                return NotFound(new { message = "Post no encontrado." });

            return Ok(commentsDto);
        }

        [Authorize]
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
    }
}
