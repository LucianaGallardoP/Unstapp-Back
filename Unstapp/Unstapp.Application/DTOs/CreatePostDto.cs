using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Application.DTOs
{
    public class CreatePostDto
    {
        public int? SubjectId { get; set; }

        [Required(ErrorMessage = "El contenido del post no puede estar vacío.")]
        public string Content { get; set; } = null!;

        [Url(ErrorMessage = "El enlace multimedia no es válido.")]
        public string? MediaUrl { get; set; }
    }
}
