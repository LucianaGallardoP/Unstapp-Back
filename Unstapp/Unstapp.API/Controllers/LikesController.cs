using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("/api/posts/{postId:int}/likes")]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized(new { message = "Token inválido." });

            if(!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Identificador de usuario inválido." });

            var result = await _likeService.ToggleLikeAsync(postId, userId);

            return result switch
            {
                ToggleLikeResult.PostNotFound => NotFound(new { message = "La publicación no existe." }),

                ToggleLikeResult.Liked => Ok(new
                {
                    message = "Like agregado correctamente.",
                    isLiked = true
                }),

                ToggleLikeResult.Unliked => Ok(new
                {
                    message = "Like eliminado correctamente.",
                    isLiked = false
                }),

                _ => StatusCode(500, new { message = "Error inesperado" })
            };

        }
    }
}
