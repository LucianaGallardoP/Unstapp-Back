using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Unstapp.Application.DTOs
{
    public class CreatePostDto
    {
        public int? SubjectId { get; set; }

        [Required(ErrorMessage = "El contenido del post no puede estar vacío.")]
        [MaxLength(500, ErrorMessage = "El contenido del post no puede superar los 500 caracteres")]
        public string Content { get; set; } = null!;

        public IFormFile? MediaFile { get; set; }
    }
}
