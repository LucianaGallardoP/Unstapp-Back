using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unstapp.Application.Interfaces;

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
    }
}
