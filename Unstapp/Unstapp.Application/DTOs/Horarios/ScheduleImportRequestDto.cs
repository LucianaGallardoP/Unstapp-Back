using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Unstapp.Application.DTOs.Horarios
{
    public class ScheduleImportRequestDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
