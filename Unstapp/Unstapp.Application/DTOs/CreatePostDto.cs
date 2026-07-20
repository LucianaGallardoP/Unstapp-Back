using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.Application.DTOs
{
    public class CreatePostDto
    {
        [MaxLength(500, ErrorMessage = "El contenido del post no puede superar los 500 caracteres")]
        public string? Content { get; set; }
        public bool IsImportant { get; set; } = false;
        public IFormFile? MediaFile { get; set; }
        public List<int>? CareerIds { get; set; }
    }
}
