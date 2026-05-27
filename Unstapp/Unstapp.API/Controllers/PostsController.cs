using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using System.Security.Claims;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetPostsByUser(int id)
        {
            var posts = await _postService.GetPostsByUserAsync(id);

            return Ok(posts);
        }


        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreatePostDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized(new { message = "Token Inválido." });

            if(!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Identificador de Usuario Inválido." });

            var post = await _postService.CreateAsync(userId, dto);

            return Ok(post);
        }
    }
}
