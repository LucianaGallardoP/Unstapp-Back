using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestAuthController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("Endpoint Publico - no necesita token");
        }

        [Authorize]
        [HttpGet("private")]
        public IActionResult Private()
        {
            return Ok("Endpoint privado - token valido");
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var dni = User.FindFirst("dni")?.Value;

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Ok(new
            {
                userId,
                dni,
                roles
            });
        }
    }
}
