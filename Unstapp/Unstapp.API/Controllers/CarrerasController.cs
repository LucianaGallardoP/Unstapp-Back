using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var result = await _careerService.GetAllCareersAsync();

            if(!result.Success)
                return StatusCode(result.Error!.StatusCode, result.Error);

            return Ok(result.Data);
        }
    }
}