using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Unstapp.Application.Interfaces;

namespace Unstapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administracion")] 
    public class CarrerasController : ControllerBase
    {
        private readonly ICareerAdminService _careerService;

        public CarrerasController(ICareerAdminService careerService)
        {
            _careerService = careerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var careers = await _careerService.GetAllCareersAsync();
            return Ok(careers);
        }
    }
}