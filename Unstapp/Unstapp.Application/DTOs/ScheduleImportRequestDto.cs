using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Unstapp.Application.DTOs
{
    public class ScheduleImportRequestDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
